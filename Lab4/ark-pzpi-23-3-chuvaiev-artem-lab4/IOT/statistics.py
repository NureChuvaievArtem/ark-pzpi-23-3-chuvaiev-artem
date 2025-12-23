import time
import math

class SystemStatistics:
    """Збір та аналіз статистики роботи NFC Mailbox системи"""
    
    def __init__(self):
        self.start_time = time.time()
        
        # Лічильники
        self.nfc_validations_success = 0
        self.nfc_validations_failed = 0
        self.packages_delivered = 0
        self.packages_received = 0
        self.lockers_opened = 0
        
        # Дані для аналізу
        self.locker_utilizations = []
        self.efficiency_scores = []
        self.operation_times = []
        
    def record_nfc_validation(self, success):
        """Записати результат валідації NFC"""
        if success:
            self.nfc_validations_success += 1
        else:
            self.nfc_validations_failed += 1
    
    def record_package_delivered(self, efficiency_score, utilization):
        """Записати доставку посилки"""
        self.packages_delivered += 1
        self.efficiency_scores.append(efficiency_score)
        self.locker_utilizations.append(utilization)
    
    def record_package_received(self):
        """Записати отримання посилки"""
        self.packages_received += 1
    
    def record_locker_opened(self):
        """Записати відкриття комірки"""
        self.lockers_opened += 1
    
    def record_operation_time(self, duration):
        """Записати час операції"""
        self.operation_times.append(duration)
    
    def get_uptime(self):
        """Отримати час роботи системи в секундах"""
        return time.time() - self.start_time
    
    def get_nfc_success_rate(self):
        """Відсоток успішних валідацій NFC"""
        total = self.nfc_validations_success + self.nfc_validations_failed
        if total == 0:
            return 0.0
        return (self.nfc_validations_success / total) * 100.0
    
    def get_average_efficiency(self):
        """Середня ефективність розміщення посилок"""
        if not self.efficiency_scores:
            return 0.0
        return sum(self.efficiency_scores) / len(self.efficiency_scores)
    
    def get_average_utilization(self):
        """Середнє використання комірок"""
        if not self.locker_utilizations:
            return 0.0
        return sum(self.locker_utilizations) / len(self.locker_utilizations)
    
    def get_std_deviation_efficiency(self):
        """Стандартне відхилення ефективності"""
        if len(self.efficiency_scores) < 2:
            return 0.0
        
        mean = self.get_average_efficiency()
        variance = sum((x - mean) ** 2 for x in self.efficiency_scores) / len(self.efficiency_scores)
        return math.sqrt(variance)
    
    def get_average_operation_time(self):
        """Середній час операції"""
        if not self.operation_times:
            return 0.0
        return sum(self.operation_times) / len(self.operation_times)
    
    def get_summary(self):
        """Отримати короткий звіт статистики"""
        uptime_hours = self.get_uptime() / 3600.0
        
        return {
            'uptime_hours': uptime_hours,
            'nfc_success_rate': self.get_nfc_success_rate(),
            'total_validations': self.nfc_validations_success + self.nfc_validations_failed,
            'packages_delivered': self.packages_delivered,
            'packages_received': self.packages_received,
            'lockers_opened': self.lockers_opened,
            'avg_efficiency': self.get_average_efficiency(),
            'std_efficiency': self.get_std_deviation_efficiency(),
            'avg_utilization': self.get_average_utilization(),
            'avg_operation_time': self.get_average_operation_time()
        }
    
    def print_summary(self):
        """Вивести звіт в консоль"""
        stats = self.get_summary()
        
        print("\n" + "="*50)
        print("SYSTEM STATISTICS SUMMARY")
        print("="*50)
        print(f"Uptime:              {stats['uptime_hours']:.2f} hours")
        print(f"NFC Success Rate:    {stats['nfc_success_rate']:.1f}%")
        print(f"Total Validations:   {stats['total_validations']}")
        print(f"Packages Delivered:  {self.packages_delivered}")
        print(f"Packages Received:   {self.packages_received}")
        print(f"Lockers Opened:      {self.lockers_opened}")
        print(f"Avg Efficiency:      {stats['avg_efficiency']:.1f}% (±{stats['std_efficiency']:.1f})")
        print(f"Avg Utilization:     {stats['avg_utilization']:.1f}%")
        
        if stats['avg_operation_time'] > 0:
            print(f"Avg Operation Time:  {stats['avg_operation_time']:.2f}s")
        
        print("="*50 + "\n")
    
    def get_lcd_summary(self):
        """Отримати короткий звіт для LCD (2 рядки по 16 символів)"""
        stats = self.get_summary()
        
        line1 = f"Pkg:{self.packages_delivered}/{self.packages_received}"
        line2 = f"Eff:{stats['avg_efficiency']:.0f}% SR:{stats['nfc_success_rate']:.0f}%"
        
        return (line1[:16], line2[:16])

