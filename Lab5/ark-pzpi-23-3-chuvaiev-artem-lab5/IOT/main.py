print("=== NFC Mailbox System Starting ===")

import network
import time
import urequests
import ujson
from machine import Pin, SPI, I2C
from api_models import (
    parse_validation_response,
    parse_packages,
    parse_lockers
)
from localization import get_text, set_language, get_language
from statistics import SystemStatistics
from config import *

# ==========================================
# LCD Display Configuration
# ==========================================
try:
    from lcd_api import LcdApi
    from i2c_lcd import I2cLcd
    
    i2c = I2C(0, scl=Pin(LCD_SCL_PIN), sda=Pin(LCD_SDA_PIN), freq=LCD_I2C_FREQ)
    lcd = I2cLcd(i2c, LCD_I2C_ADDRESS, LCD_ROWS, LCD_COLS)
    LCD_AVAILABLE = True
    print("LCD Display initialized")
except Exception as e:
    print(f"LCD not available: {e}")
    LCD_AVAILABLE = False
    lcd = None

# ==========================================
# Keypad Configuration (4x4 Matrix)
# ==========================================
row_pins = [Pin(pin, Pin.OUT) for pin in KEYPAD_ROWS]
col_pins = [Pin(pin, Pin.IN, Pin.PULL_DOWN) for pin in KEYPAD_COLS]

def read_keypad():
    """Считывает нажатую клавишу с матричной клавиатуры"""
    for row_idx, row_pin in enumerate(row_pins):
        row_pin.on()
        time.sleep(0.001)
        
        for col_idx, col_pin in enumerate(col_pins):
            if col_pin.value() == 1:
                key = KEYPAD_KEYS[row_idx][col_idx]
                row_pin.off()
                while col_pin.value() == 1:
                    time.sleep(0.01)
                return key
        
        row_pin.off()
    
    return None

# ==========================================
# LCD Helper Functions
# ==========================================

def lcd_clear():
    """Очищает LCD дисплей"""
    if LCD_AVAILABLE:
        lcd.clear()

def lcd_print(line1, line2=""):
    """Выводит текст на LCD дисплей (2 строки по 16 символов)"""
    if LCD_AVAILABLE:
        lcd.clear()
        lcd.move_to(0, 0)
        lcd.putstr(line1[:16]) 
        if line2:
            lcd.move_to(0, 1)
            lcd.putstr(line2[:16])
    print(f"LCD: {line1}")
    if line2:
        print(f"     {line2}")

def lcd_input(prompt, max_length=None, numeric_only=None):
    if max_length is None:
        max_length = MAX_SERIAL_LENGTH
    if numeric_only is None:
        numeric_only = NUMERIC_ONLY_INPUT
    input_buffer = ""
    lcd_print(prompt[:16], f"> {input_buffer}")
    
    while True:
        key = read_keypad()
        
        if key:
            if key == '#':
                if len(input_buffer) > 0:
                    return input_buffer
                else:
                    lcd_print(get_text("empty_input"), get_text("try_again"))
                    time.sleep(1)
                    lcd_print(prompt[:16], f"> {input_buffer}")
            
            elif key == '*':
                if len(input_buffer) > 0:
                    input_buffer = input_buffer[:-1]
                    lcd_print(prompt[:16], f"> {input_buffer}")
            
            elif key == 'D':
                return None
            
            elif key in ['A', 'B', 'C']:
                pass
            
            else:
                if numeric_only and key not in '0123456789':
                    continue
                
                if len(input_buffer) < max_length:
                    input_buffer += key
                    display_text = input_buffer
                    if len(display_text) > 14:
                        display_text = ".." + display_text[-12:]
                    lcd_print(prompt[:16], f"> {display_text}")
        
        time.sleep(0.1)

def lcd_menu(title, options):
    while True:
        lcd_print(title[:16], get_text("select_option"))
        time.sleep(1)
        
        for i in range(0, len(options), 2):
            line1 = f"{i+1}.{options[i][:14]}" if i < len(options) else ""
            line2 = f"{i+2}.{options[i+1][:14]}" if i+1 < len(options) else ""
            lcd_print(line1, line2)
            
            start_time = time.time()
            while time.time() - start_time < MENU_DISPLAY_DURATION:  
                key = read_keypad()
                if key:
                    if key == 'D':
                        return None
                    
                    if key.isdigit():
                        choice = int(key) - 1
                        if 0 <= choice < len(options):
                            return choice
                
                time.sleep(KEY_DEBOUNCE_DELAY)

# ==========================================
# WiFi Configuration
# ==========================================
print("Connecting to WiFi", end="")
lcd_print(get_text("connecting_wifi"), get_text("please_wait"))
sta_if = network.WLAN(network.STA_IF)
sta_if.active(True)
sta_if.connect(WIFI_SSID, WIFI_PASSWORD)
while not sta_if.isconnected():
    print(".", end="")
    time.sleep(0.1)
print(" Connected!")
print("IP:", sta_if.ifconfig()[0])
lcd_print(get_text("wifi_connected"), sta_if.ifconfig()[0])
time.sleep(2)

# ==========================================
# Hardware Configuration
# ==========================================
LED_SUCCESS = Pin(LED_SUCCESS_PIN, Pin.OUT)  
LED_ERROR = Pin(LED_ERROR_PIN, Pin.OUT)   
RELAY = Pin(RELAY_PIN, Pin.OUT)

print(f"Loaded {len(LOCKER_DATABASE)} lockers into local database")

# ==========================================
# STATISTICS MODULE
# ==========================================
stats = SystemStatistics()
print("Statistics module initialized")

# ==========================================
# Helper Functions
# ==========================================

def blink_led(led, times=None, delay=None):
    if times is None:
        times = LED_SUCCESS_BLINK_COUNT
    if delay is None:
        delay = LED_BLINK_DELAY
    for _ in range(times):
        led.on()
        time.sleep(delay)
        led.off()
        time.sleep(delay)

def update_locker_state(locker_id, package_volume):
    """Оновлює стан комірки в локальній базі даних"""
    for locker in LOCKER_DATABASE:
        if locker['id'] == locker_id:
            locker['currentUsage'] += package_volume
            utilization = (locker['currentUsage'] / locker['maxVolume']) * 100
            print(f"[DB UPDATE] Locker {locker_id}: usage = {locker['currentUsage']}/{locker['maxVolume']} ({utilization:.2f}%)")
            locker['status'] = 'occupied'
            print(f"[DB UPDATE] Locker {locker_id}: status changed to 'occupied'")
            return True
    
    print(f"[DB ERROR] Locker {locker_id} not found in database")
    return False

def clear_locker_state(locker_id):
    """Очищує стан комірки"""
    for locker in LOCKER_DATABASE:
        if locker['id'] == locker_id:
            old_usage = locker['currentUsage']
            locker['currentUsage'] = 0
            locker['status'] = 'available'
            print(f"[DB UPDATE] Locker {locker_id}: cleared (was {old_usage} mm³)")
            return True
    
    print(f"[DB ERROR] Locker {locker_id} not found in database")
    return False

def open_locker(locker_number, duration=None):
    if duration is None:
        duration = LOCKER_OPEN_DURATION
    """Відкриває замок на вказаний час"""
    print(f"Opening locker {locker_number}...")
    lcd_print(get_text("opening_locker"), f"#{locker_number}...")
    RELAY.on()
    blink_led(LED_SUCCESS, 2, 0.3)
    time.sleep(duration)
    RELAY.off()
    print("Locker closed")
    lcd_print(get_text("locker_closed"), "")
    stats.record_locker_opened()

def calculate_optimal_locker(package_height, package_width, package_depth, available_lockers=None):
    """Алгоритм оптимального розміщення"""
    if available_lockers is None:
        available_lockers = [locker for locker in LOCKER_DATABASE if locker['status'] == 'available']
    
    print("\n=== OPTIMAL PLACEMENT CALCULATION ===")
    package_volume = package_height * package_width * package_depth
    print(f"Package volume: {package_volume} mm³")
    
    sorted_lockers = sorted(available_lockers, key=lambda x: x['currentUsage'])
    
    best_locker = None
    best_efficiency = -1
    
    for locker in sorted_lockers:
        locker_id = locker['id']
        max_volume = locker['maxVolume']
        current_usage = locker['currentUsage']
        
        locker_height = locker.get('height', 0)
        locker_width = locker.get('width', 0)
        locker_depth = locker.get('depth', 0)
        
        available_space = max_volume - current_usage
        
        if current_usage > 0:
            continue
        
        package_dims = sorted([package_height, package_width, package_depth])
        locker_dims = sorted([locker_height, locker_width, locker_depth])
        
        dimensions_fit = all(p <= l for p, l in zip(package_dims, locker_dims))
        
        if not dimensions_fit:
            continue
        
        if available_space >= package_volume:
            new_usage = current_usage + package_volume
            utilization_percent = (new_usage / max_volume) * 100.0
            
            if OPTIMAL_UTILIZATION_MIN <= utilization_percent <= OPTIMAL_UTILIZATION_MAX:
                efficiency_score = 100.0
            elif utilization_percent < OPTIMAL_UTILIZATION_MIN:
                efficiency_score = 70.0 + (utilization_percent / OPTIMAL_UTILIZATION_MIN) * 30.0
            else:
                penalty = ((utilization_percent - OPTIMAL_UTILIZATION_MAX) / 15.0) * 30.0
                efficiency_score = 100.0 - penalty
            
            efficiency_score = max(0.0, min(100.0, efficiency_score))
            
            if efficiency_score > best_efficiency:
                best_efficiency = efficiency_score
                best_locker = {
                    'lockerId': locker_id,
                    'utilization': utilization_percent,
                    'efficiency': efficiency_score,
                    'volume': package_volume
                }
    
    return best_locker

# ==========================================
# API Functions
# ==========================================

def validate_nfc(serial_number):
    """Валідує NFC картку через API"""
    try:
        url = f"{API_BASE_URL}/api/Nfc/validate"
        headers = {'Content-Type': 'application/json'}
        payload = {"serialNumber": serial_number}
        
        print(f"Validating NFC: {serial_number}")
        response = urequests.post(url, json=payload, headers=headers)
        
        if response.status_code == 200:
            raw_data = ujson.loads(response.text)
            response.close()
            
            print(f"[API] Raw response: {raw_data}")
            
            parsed = parse_validation_response(raw_data)
            return parsed
        else:
            print(f"Validation failed: {response.status_code}")
            response.close()
            return None
            
    except Exception as e:
        print(f"API Error: {e}")
        return None

def get_courier_packages(serial_number):
    """Отримує список пакунків для кур'єра"""
    try:
        url = f"{API_BASE_URL}/api/Package/courier?serialNumber={serial_number}"
        response = urequests.get(url)
        
        if response.status_code == 200:
            raw_data = ujson.loads(response.text)
            response.close()
            
            print(f"[API] Raw packages: {raw_data}")
            
            packages = parse_packages(raw_data)
            return packages
        else:
            response.close()
            return None
            
    except Exception as e:
        print(f"API Error: {e}")
        return None

def place_package(package_id, postbox_id, serial_number):
    """Відмічає пакунок як розміщений"""
    try:
        url = f"{API_BASE_URL}/api/Package/place"
        headers = {'Content-Type': 'application/json'}
        payload = {
            "packageId": package_id,
            "postBoxId": postbox_id,
            "serialNumber": serial_number
        }
        
        response = urequests.post(url, json=payload, headers=headers)
        
        if response.status_code == 200:
            response.close()
            return True
        else:
            response.close()
            return False
            
    except Exception as e:
        print(f"API Error: {e}")
        return False

def get_delivered_lockers(serial_number):
    """Отримує список комірок з доставленими пакунками"""
    try:
        url = f"{API_BASE_URL}/api/Package/locker/open-all-delivered"
        headers = {'Content-Type': 'application/json'}
        payload = {"serialNumber": serial_number}
        
        response = urequests.post(url, json=payload, headers=headers)
        
        if response.status_code == 200:
            raw_data = ujson.loads(response.text)
            response.close()
            
            print(f"[API] Raw lockers: {raw_data}")
            
            lockers = parse_lockers(raw_data)
            return lockers
        else:
            response.close()
            return None
            
    except Exception as e:
        print(f"API Error: {e}")
        return None

def mark_package_received(package_id, serial_number):
    """Відмічає посилку як отриману"""
    try:
        url = f"{API_BASE_URL}/api/Package/{package_id}/receive?serialNumber={serial_number}"
        
        print(f"[API] Marking package {package_id} as received")
        print(f"[API] URL: {url}")
        
        headers = {'Content-Length': '0'}
        response = urequests.post(url, headers=headers)
        
        print(f"[API] Response status: {response.status_code}")
        
        if response.status_code == 200:
            print(f"[API] Package {package_id} marked as received")
            response.close()
            return True
        else:
            try:
                error_text = response.text
                print(f"[API] Error response: {error_text}")
            except:
                pass
            response.close()
            return False
            
    except Exception as e:
        print(f"[API] Exception: {e}")
        return False

# ==========================================
# STATE MACHINE
# ==========================================

STATE_IDLE = "IDLE"
STATE_MAIN_MENU = "MAIN_MENU"
STATE_INPUT_SERIAL = "INPUT_SERIAL"
STATE_COURIER_MODE = "COURIER_MODE"
STATE_CLIENT_MODE = "CLIENT_MODE"
STATE_PROCESSING = "PROCESSING"
STATE_ERROR = "ERROR"

class MailboxStateMachine:
    """Машина состояний для системы почтовых ящиков"""
    
    def __init__(self):
        self.state = STATE_IDLE
        self.serial_number = None
        self.user_data = None
        self.error_message = None
        
    def transition_to(self, new_state, data=None):
        """Переход в новое состояние"""
        print(f"\n[STATE TRANSITION] {self.state} -> {new_state}")
        self.state = new_state
        if data:
            print(f"[STATE DATA] {data}")
    
    def run(self):
        """Главный цикл машины состояний"""
        while True:
            try:
                if self.state == STATE_IDLE:
                    self.handle_idle()
                
                elif self.state == STATE_MAIN_MENU:
                    self.handle_main_menu()
                
                elif self.state == STATE_INPUT_SERIAL:
                    self.handle_input_serial()
                
                elif self.state == STATE_COURIER_MODE:
                    self.handle_courier_mode()
                
                elif self.state == STATE_CLIENT_MODE:
                    self.handle_client_mode()
                
                elif self.state == STATE_PROCESSING:
                    self.handle_processing()
                
                elif self.state == STATE_ERROR:
                    self.handle_error()
                
                else:
                    print(f"Unknown state: {self.state}")
                    self.transition_to(STATE_IDLE)
                
                time.sleep(0.1)
                
            except KeyboardInterrupt:
                print("\nShutting down...")
                lcd_print("System", get_text("shutting_down"))
                break
            except Exception as e:
                print(f"Error in state machine: {e}")
                self.error_message = str(e)
                self.transition_to(STATE_ERROR)
    
    def handle_idle(self):
        """IDLE состояние - ожидание действия"""
        lcd_print("NFC Mailbox", get_text("press_any_key"))
        print("System ready. Waiting for input...")
        
        key = None
        while not key:
            key = read_keypad()
            time.sleep(0.1)
        
        blink_led(LED_SUCCESS, 1, 0.1)
        self.transition_to(STATE_MAIN_MENU)
    
    def handle_main_menu(self):
        """MAIN MENU состояние - главное меню"""
        options = [
            get_text("courier_mode"),
            get_text("client_mode"),
            "EN/UK",
            "Statistics"
        ]
        
        choice = lcd_menu(get_text("main_menu"), options)
        
        if choice is None:
            self.transition_to(STATE_IDLE)
        elif choice == 0:
            self.transition_to(STATE_INPUT_SERIAL, {"mode": "courier"})
        elif choice == 1:
            self.transition_to(STATE_INPUT_SERIAL, {"mode": "client"})
        elif choice == 2:
            # Toggle language
            current = get_language()
            new_lang = "uk" if current == "en" else "en"
            set_language(new_lang)
            lcd_print("Language:", "EN" if new_lang == "en" else "Ukrainian")
            time.sleep(2)
        elif choice == 3:
            # Show statistics
            self.show_statistics()
    
    def handle_input_serial(self):
        """INPUT SERIAL состояние - ввод серийного номера"""
        mode = getattr(self, 'mode', 'unknown')
        
        lcd_print(get_text("enter_serial"), get_text("ok_back"))
        time.sleep(1)
        
        serial = lcd_input(get_text("serial_number"), max_length=16)
        
        if serial is None:
            lcd_print(get_text("cancelled"), "")
            time.sleep(1)
            self.transition_to(STATE_MAIN_MENU)
            return
        
        self.serial_number = serial
        
        lcd_print(get_text("validating"), get_text("please_wait"))
        validation_result = validate_nfc(serial)
        
        if validation_result:
            self.user_data = validation_result
            stats.record_nfc_validation(True)
            
            lcd_print(get_text("valid"), f"{get_text('user')} {validation_result.name[:8]}")
            blink_led(LED_SUCCESS, 2, 0.2)
            time.sleep(2)
            
            if validation_result.has_role('Courier'):
                self.transition_to(STATE_COURIER_MODE)
            elif validation_result.has_role('Client'):
                self.transition_to(STATE_CLIENT_MODE)
            else:
                self.error_message = f"Unknown role: {validation_result.roles}"
                self.transition_to(STATE_ERROR)
        else:
            stats.record_nfc_validation(False)
            self.error_message = get_text("invalid")
            self.transition_to(STATE_ERROR)
    
    def handle_courier_mode(self):
        """COURIER MODE состояние - режим курьера"""
        lcd_print(get_text("courier_mode"), get_text("loading"))
        print("\n" + "="*70)
        print("COURIER MODE ACTIVATED")
        print("="*70)
        
        packages = get_courier_packages(self.serial_number)
        
        if not packages or len(packages) == 0:
            lcd_print(get_text("no_packages"), get_text("to_deliver"))
            blink_led(LED_ERROR, 2, 0.3)
            time.sleep(2)
            self.transition_to(STATE_MAIN_MENU)
            return
        
        lcd_print(f"{get_text('found')} {len(packages)} {get_text('pkg')}", get_text("processing"))
        print(f"\nFound {len(packages)} packages to deliver")
        time.sleep(2)
        
        for idx, package in enumerate(packages):
            lcd_print(f"{get_text('package')} {idx+1}/{len(packages)}", f"ID: {package.id}")
            time.sleep(2)
            
            print(f"\n{'='*50}")
            print(f"Processing package ID: {package.id}")
            print(f"Dimensions: {package.height}x{package.width}x{package.depth} mm")
            
            optimal = calculate_optimal_locker(
                package.height, 
                package.width, 
                package.depth
            )
            
            if optimal:
                locker_id = optimal['lockerId']
                
                lcd_print(f"{get_text('use_locker')} #{locker_id}", f"{get_text('eff')} {optimal['efficiency']:.0f}%")
                blink_led(LED_SUCCESS, 3, 0.3)
                time.sleep(3)
                
                lcd_print(f"{get_text('place_in')} #{locker_id}", get_text("done"))
                
                # Чекаємо підтвердження з timeout
                key = None
                timeout_start = time.time()
                timeout_duration = OPERATION_TIMEOUT
                
                while key != '#':
                    if time.time() - timeout_start > timeout_duration:
                        lcd_print("Timeout!", get_text("auto_skip"))
                        time.sleep(2)
                        break
                        
                    key = read_keypad()
                    if key == 'D':
                        lcd_print(get_text("skipped"), "")
                        time.sleep(1)
                        break
                    time.sleep(0.1)
                
                if key == '#':
                    lcd_print(get_text("confirming"), "")
                    if place_package(package.id, locker_id, self.serial_number):
                        update_locker_state(locker_id, package.volume)
                        stats.record_package_delivered(optimal['efficiency'], optimal['utilization'])
                        
                        lcd_print(get_text("success"), f"{get_text('locker')} #{locker_id}")
                        blink_led(LED_SUCCESS, 5, 0.2)
                        time.sleep(2)
                    else:
                        lcd_print(get_text("api_error"), get_text("failed_to_save"))
                        blink_led(LED_ERROR, 3, 0.3)
                        time.sleep(2)
            else:
                lcd_print(get_text("no_locker"), get_text("available"))
                blink_led(LED_ERROR, 5, 0.2)
                time.sleep(2)
        
        lcd_print(get_text("all_done"), f"{len(packages)} {get_text('delivered')}")
        blink_led(LED_SUCCESS, 10, 0.1)
        time.sleep(3)
        
        self.transition_to(STATE_MAIN_MENU)
    
    def handle_client_mode(self):
        """CLIENT MODE состояние - режим клиента"""
        lcd_print(get_text("client_mode"), get_text("loading"))
        print("\n=== CLIENT MODE ===")
        
        lockers = get_delivered_lockers(self.serial_number)
        
        if not lockers or len(lockers) == 0:
            lcd_print(get_text("no_packages"), get_text("available"))
            blink_led(LED_ERROR, 2, 0.3)
            time.sleep(2)
            self.transition_to(STATE_MAIN_MENU)
            return
        
        lcd_print(f"{get_text('found')} {len(lockers)} {get_text('pkg')}", get_text("opening"))
        print(f"Found {len(lockers)} lockers with packages")
        time.sleep(2)
        
        for idx, locker_package in enumerate(lockers):
            locker_id = locker_package.locker_id
            package_id = locker_package.package_id
            
            lcd_print(f"{get_text('locker')} {idx+1}/{len(lockers)}", f"#{locker_id} {get_text('opening')}")
            time.sleep(1)
            
            print(f"Opening locker {locker_id}...")
            RELAY.on()
            blink_led(LED_SUCCESS, 2, 0.3)
            
            lcd_print(f"{get_text('take_from')} #{locker_id}", get_text("done"))
            
            # Чекаємо підтвердження з timeout
            key = None
            timeout_start = time.time()
            timeout_duration = OPERATION_TIMEOUT
            
            while key != '#':
                if time.time() - timeout_start > timeout_duration:
                    lcd_print("Timeout!", get_text("auto_close"))
                    time.sleep(2)
                    break
                    
                key = read_keypad()
                if key == 'D':
                    lcd_print(get_text("skipped"), "")
                    time.sleep(1)
                    break
                time.sleep(0.1)
            
            RELAY.off()
            
            if key == '#':
                lcd_print(get_text("confirming"), "")
                if package_id and mark_package_received(package_id, self.serial_number):
                    clear_locker_state(locker_id)
                    stats.record_package_received()
                    
                    lcd_print(get_text("received"), f"{get_text('locker')} #{locker_id}")
                    blink_led(LED_SUCCESS, 3, 0.2)
                    time.sleep(2)
                else:
                    lcd_print(get_text("api_error"), get_text("try_again_later"))
                    blink_led(LED_ERROR, 3, 0.3)
                    time.sleep(2)
        
        lcd_print(get_text("all_done"), get_text("have_nice_day"))
        blink_led(LED_SUCCESS, 10, 0.1)
        time.sleep(3)
        
        self.transition_to(STATE_MAIN_MENU)
    
    def handle_processing(self):
        """PROCESSING состояние - обработка"""
        lcd_print(get_text("processing"), get_text("please_wait"))
        time.sleep(2)
        self.transition_to(STATE_MAIN_MENU)
    
    def handle_error(self):
        """ERROR состояние - обработка ошибок"""
        error_msg = self.error_message or get_text("unknown_error")
        lcd_print(get_text("error"), error_msg[:16])
        blink_led(LED_ERROR, 3, 0.3)
        print(f"\nERROR: {error_msg}")
        time.sleep(3)
        
        self.error_message = None
        self.serial_number = None
        self.user_data = None
        
        self.transition_to(STATE_MAIN_MENU)
    
    def show_statistics(self):
        """Відображає статистику на LCD"""
        stats.print_summary()
        
        summary = stats.get_summary()
        
        # Screen 1: Uptime & Validations
        lcd_print("Statistics:", "")
        time.sleep(1)
        lcd_print(f"Uptime:{summary['uptime_hours']:.1f}h", f"Valid:{summary['nfc_success_rate']:.0f}%")
        time.sleep(3)
        
        # Screen 2: Packages
        lcd_print(f"Delivered:{stats.packages_delivered}", f"Received:{stats.packages_received}")
        time.sleep(3)
        
        # Screen 3: Efficiency & Utilization
        if summary['avg_efficiency'] > 0:
            lcd_print(f"AvgEff:{summary['avg_efficiency']:.0f}%", f"AvgUtil:{summary['avg_utilization']:.0f}%")
            time.sleep(3)
        
        # Screen 4: Lockers opened
        lcd_print(f"Lockers:{stats.lockers_opened}", "")
        time.sleep(3)
        
        lcd_print("Press any key", "to continue...")
        key = None
        while not key:
            key = read_keypad()
            time.sleep(0.1)

# ==========================================
# Main Entry Point
# ==========================================

# Set default language from config
set_language(DEFAULT_LANGUAGE)

print("=== System Ready ===")
lcd_print(get_text("system_ready"), get_text("starting"))
time.sleep(2)

state_machine = MailboxStateMachine()
state_machine.run()

# Cleanup
LED_SUCCESS.off()
LED_ERROR.off()
RELAY.off()
lcd_clear()
print("System stopped")