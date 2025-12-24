"""I2C LCD driver for MicroPython"""

import time
from lcd_api import LcdApi

# PCF8574 pin definitions
MASK_RS = 0x01       # P0
MASK_RW = 0x02       # P1
MASK_E = 0x04        # P2
MASK_BACKLIGHT = 0x08  # P3

SHIFT_DATA = 4       # P4-P7

class I2cLcd(LcdApi):
    """I2C LCD driver. Implements the LcdApi interface using I2C."""
    
    def __init__(self, i2c, i2c_addr, num_lines, num_columns):
        """Initialize I2C LCD.
        
        Args:
            i2c: I2C bus object
            i2c_addr: I2C address of the LCD (typically 0x27 or 0x3F)
            num_lines: Number of lines (2 or 4)
            num_columns: Number of columns (16 or 20)
        """
        self.i2c = i2c
        self.i2c_addr = i2c_addr
        self.i2c.writeto(self.i2c_addr, bytearray([0]))
        time.sleep_ms(20)  # Allow LCD time to power up
        
        # Initialize LCD in 4-bit mode
        # This is the standard initialization sequence
        self.hal_write_init_nibble(self.LCD_FUNCTION_RESET)
        time.sleep_ms(5)
        self.hal_write_init_nibble(self.LCD_FUNCTION_RESET)
        time.sleep_ms(1)
        self.hal_write_init_nibble(self.LCD_FUNCTION_RESET)
        time.sleep_ms(1)
        
        # Put LCD into 4-bit mode
        self.hal_write_init_nibble(self.LCD_FUNCTION)
        time.sleep_ms(1)
        
        # Now we can use the standard API
        LcdApi.__init__(self, num_lines, num_columns)
        cmd = self.LCD_FUNCTION
        if num_lines > 1:
            cmd |= self.LCD_FUNCTION_2LINES
        self.hal_write_command(cmd)
    
    def hal_write_init_nibble(self, nibble):
        """Write an initialization nibble to the LCD."""
        byte = ((nibble >> 4) & 0x0f) << SHIFT_DATA
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_E | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_BACKLIGHT]))
        time.sleep_ms(1)
    
    def hal_backlight_on(self):
        """Turn on the backlight."""
        self.i2c.writeto(self.i2c_addr, bytearray([MASK_BACKLIGHT]))
    
    def hal_backlight_off(self):
        """Turn off the backlight."""
        self.i2c.writeto(self.i2c_addr, bytearray([0]))
    
    def hal_write_command(self, cmd):
        """Write a command to the LCD."""
        byte = ((cmd >> 4) & 0x0f) << SHIFT_DATA
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_E | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        
        byte = (cmd & 0x0f) << SHIFT_DATA
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_E | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        
        if cmd <= 3:
            # Home and clear commands need more time
            time.sleep_ms(5)
    
    def hal_write_data(self, data):
        """Write data to the LCD."""
        byte = (MASK_RS | ((data >> 4) & 0x0f) << SHIFT_DATA)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_E | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        
        byte = (MASK_RS | (data & 0x0f) << SHIFT_DATA)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_E | MASK_BACKLIGHT]))
        time.sleep_ms(1)
        self.i2c.writeto(self.i2c_addr, bytearray([byte | MASK_BACKLIGHT]))
        time.sleep_ms(1)

