"""
Configuration loader for NFC Mailbox IOT System
Loads configuration from config.json file
"""

import ujson

# Load configuration from JSON file
try:
    with open('config.json', 'r') as f:
        _config = ujson.load(f)
    print("Configuration loaded from config.json")
except Exception as e:
    print(f"Error loading config.json: {e}")
    _config = {}

# WiFi Configuration
WIFI_SSID = _config.get('wifi', {}).get('ssid', 'Wokwi-GUEST')
WIFI_PASSWORD = _config.get('wifi', {}).get('password', '')

# API Configuration
API_BASE_URL = _config.get('api', {}).get('base_url', '')

# LCD Display Configuration
LCD_I2C_ADDRESS = _config.get('lcd', {}).get('i2c_address', 0x27)
LCD_ROWS = _config.get('lcd', {}).get('rows', 2)
LCD_COLS = _config.get('lcd', {}).get('cols', 16)
LCD_SCL_PIN = _config.get('lcd', {}).get('scl_pin', 22)
LCD_SDA_PIN = _config.get('lcd', {}).get('sda_pin', 21)
LCD_I2C_FREQ = _config.get('lcd', {}).get('i2c_freq', 400000)

# Keypad Configuration
KEYPAD_ROWS = _config.get('keypad', {}).get('rows', [13, 12, 14, 27])
KEYPAD_COLS = _config.get('keypad', {}).get('cols', [26, 25, 33, 32])
KEYPAD_KEYS = _config.get('keypad', {}).get('keys', [
    ['1', '2', '3', 'A'],
    ['4', '5', '6', 'B'],
    ['7', '8', '9', 'C'],
    ['*', '0', '#', 'D']
])

# Hardware Pin Configuration
LED_SUCCESS_PIN = _config.get('hardware', {}).get('led_success_pin', 2)
LED_ERROR_PIN = _config.get('hardware', {}).get('led_error_pin', 4)
RELAY_PIN = _config.get('hardware', {}).get('relay_pin', 15)

# Localization
DEFAULT_LANGUAGE = _config.get('localization', {}).get('default_language', 'en')

# Timeout Settings
OPERATION_TIMEOUT = _config.get('timeouts', {}).get('operation_timeout', 30)
LOCKER_OPEN_DURATION = _config.get('timeouts', {}).get('locker_open_duration', 5)

# Input Settings
MAX_SERIAL_LENGTH = _config.get('input', {}).get('max_serial_length', 16)
NUMERIC_ONLY_INPUT = _config.get('input', {}).get('numeric_only', False)

# Locker Database
LOCKER_DATABASE = _config.get('lockers', [])

# Optimal Placement Algorithm Settings
OPTIMAL_UTILIZATION_MIN = _config.get('algorithm', {}).get('optimal_utilization_min', 60)
OPTIMAL_UTILIZATION_MAX = _config.get('algorithm', {}).get('optimal_utilization_max', 85)

# Timing Settings
MENU_DISPLAY_DURATION = _config.get('timing', {}).get('menu_display_duration', 3)
KEY_DEBOUNCE_DELAY = _config.get('timing', {}).get('key_debounce_delay', 0.1)
FEEDBACK_DISPLAY_DURATION = _config.get('timing', {}).get('feedback_display_duration', 2)

# LED Blink Patterns
LED_SUCCESS_BLINK_COUNT = _config.get('led_patterns', {}).get('success_blink_count', 3)
LED_ERROR_BLINK_COUNT = _config.get('led_patterns', {}).get('error_blink_count', 3)
LED_BLINK_DELAY = _config.get('led_patterns', {}).get('blink_delay', 0.2)

def get_config():
    """Returns the full configuration dictionary"""
    return _config

def reload_config():
    """Reloads configuration from config.json"""
    global _config
    try:
        with open('config.json', 'r') as f:
            _config = ujson.load(f)
        print("Configuration reloaded successfully")
        return True
    except Exception as e:
        print(f"Error reloading config: {e}")
        return False

