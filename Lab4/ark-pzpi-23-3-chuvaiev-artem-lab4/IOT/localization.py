"""
Localization system for IOT device
Supports: English and Ukrainian (transliterated to ASCII for LCD compatibility)
"""

# English translations
TRANSLATIONS_EN = {
    # System messages
    "system_starting": "System Starting",
    "system_ready": "System Ready",
    "starting": "Starting...",
    "shutting_down": "Shutting down",
    "press_any_key": "Press any key",
    
    # WiFi
    "connecting_wifi": "Connecting WiFi",
    "please_wait": "Please wait...",
    "wifi_connected": "WiFi Connected!",
    
    # Main menu
    "main_menu": "Main Menu",
    "select_option": "Select option:",
    "courier_mode": "Courier Mode",
    "client_mode": "Client Mode",
    
    # Serial input
    "enter_serial": "Enter Serial",
    "serial_number": "Serial Number:",
    "ok_back": "# = OK, D = Back",
    "cancelled": "Cancelled",
    "empty_input": "Empty input!",
    "try_again": "Try again...",
    
    # Validation
    "validating": "Validating...",
    "valid": "Valid!",
    "invalid": "Invalid serial",
    "user": "User:",
    
    # Courier mode
    "loading": "Loading...",
    "no_packages": "No packages",
    "to_deliver": "to deliver",
    "found": "Found",
    "pkg": "pkg",
    "processing": "Processing...",
    "package": "Package",
    "use_locker": "Use Locker",
    "eff": "Eff:",
    "place_in": "Place in",
    "done": "# = Done",
    "skipped": "Skipped",
    "confirming": "Confirming...",
    "success": "Success!",
    "locker": "Locker",
    "api_error": "API Error",
    "failed_to_save": "Failed to save",
    "no_locker": "No locker",
    "available": "available!",
    "all_done": "All done!",
    "delivered": "delivered",
    
    # Client mode
    "opening": "Opening...",
    "take_from": "Take from",
    "auto_close": "Auto-closing",
    "auto_skip": "Auto-skipped",
    "received": "Received!",
    "try_again_later": "Try again later",
    "have_nice_day": "Have a nice day",
    
    # Locker operations
    "opening_locker": "Opening Locker",
    "locker_closed": "Locker closed",
    
    # Errors
    "error": "Error!",
    "unknown_error": "Unknown error"
}

# Ukrainian translations (Transliterated to ASCII for LCD)
TRANSLATIONS_UK = {
    # System messages
    "system_starting": "Zapusk systemy",
    "system_ready": "Systema hotova",
    "starting": "Zapusk...",
    "shutting_down": "Vymykannia",
    "press_any_key": "Natysnit klavu",
    
    # WiFi
    "connecting_wifi": "Pidkljuchennia",
    "please_wait": "Pochekajte...",
    "wifi_connected": "WiFi pidkljuchen",
    
    # Main menu
    "main_menu": "Holovne menu",
    "select_option": "Oberit opciju:",
    "courier_mode": "Rezhym kuriera",
    "client_mode": "Rezhym klijenta",
    
    # Serial input
    "enter_serial": "Vvedit serial",
    "serial_number": "Serial nomer:",
    "ok_back": "# = OK, D = Nazad",
    "cancelled": "Skasovano",
    "empty_input": "Porozhni dani!",
    "try_again": "Sprobujte znovu",
    
    # Validation
    "validating": "Perevirka...",
    "valid": "Virnyi!",
    "invalid": "Nevirnyi serial",
    "user": "Korystuvach:",
    
    # Courier mode
    "loading": "Zavantazhennia..",
    "no_packages": "Nema posylok",
    "to_deliver": "do dostavky",
    "found": "Znaideno",
    "pkg": "pos",
    "processing": "Obrobka...",
    "package": "Posylka",
    "use_locker": "Vykoryst komirku",
    "eff": "Efekt:",
    "place_in": "Pokladit v",
    "done": "# = Hotovo",
    "skipped": "Propushcheno",
    "confirming": "Pidtverdzennia..",
    "success": "Uspikh!",
    "locker": "Komirka",
    "api_error": "Pomylka API",
    "failed_to_save": "Ne zberehlos",
    "no_locker": "Nema komirky",
    "available": "dostupnoi!",
    "all_done": "Vse hotovo!",
    "delivered": "dostavleno",
    
    # Client mode
    "opening": "Vidkryttia...",
    "take_from": "Viimit z",
    "auto_close": "Avto-zakr.",
    "auto_skip": "Avto-propusk",
    "received": "Otrymano!",
    "try_again_later": "Sprobujte piznishe",
    "have_nice_day": "Haroho dnia",
    
    # Locker operations
    "opening_locker": "Vidkryttia",
    "locker_closed": "Komirka zachynena",
    
    # Errors
    "error": "Pomylka!",
    "unknown_error": "Nevid pomylka"
}

# Current language setting
current_language = "en"

def set_language(lang_code):
    """Set current language"""
    global current_language
    if lang_code in ["en", "uk"]:
        current_language = lang_code
        print(f"[LANG] Language set to: {lang_code}")
    else:
        print(f"[LANG] Unknown language: {lang_code}, using 'en'")
        current_language = "en"

def get_text(key):
    """Get localized text by key"""
    translations = TRANSLATIONS_UK if current_language == "uk" else TRANSLATIONS_EN
    
    if key in translations:
        return translations[key]
    
    # Fallback to English
    if key in TRANSLATIONS_EN:
        return TRANSLATIONS_EN[key]
    
    # Return key if not found
    return key

def get_language():
    """Get current language"""
    return current_language

