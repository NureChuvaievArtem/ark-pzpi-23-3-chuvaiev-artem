python -m mpremote connect port:rfc2217://localhost:4000 fs cp lcd_api.py :lcd_api.py
python -m mpremote connect port:rfc2217://localhost:4000 fs cp i2c_lcd.py :i2c_lcd.py
python -m mpremote connect port:rfc2217://localhost:4000 fs cp api_models.py :api_models.py
python -m mpremote connect port:rfc2217://localhost:4000 fs cp localization.py :localization.py
python -m mpremote connect port:rfc2217://localhost:4000 fs cp statistics.py :statistics.py
python -m mpremote connect port:rfc2217://localhost:4000 fs cp main.py :main.py

python -m mpremote connect port:rfc2217://localhost:4000 exec "import main"