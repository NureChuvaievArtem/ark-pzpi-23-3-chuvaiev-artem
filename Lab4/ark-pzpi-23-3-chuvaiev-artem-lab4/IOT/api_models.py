class ValidationResponse:
    def __init__(self, data):
        if isinstance(data, dict):
            self.user_id = data.get('id') or data.get('userId')
            self.name = data.get('name') or data.get('userName') or data.get('emailAddress', 'Unknown')
            self.email = data.get('email') or data.get('emailAddress', '')
            self.serial = data.get('serialNfcData', '')
            
            raw_roles = data.get('roles', [])
            self.roles = self._parse_roles(raw_roles)
            
            self.is_valid = data.get('isValid', True)
        else:
            self.is_valid = False
            self.roles = []
            self.name = 'Unknown'
    
    def _parse_roles(self, raw_roles):
        roles = []
        
        if not raw_roles:
            return roles
        
        for role_item in raw_roles:
            if isinstance(role_item, str):
                roles.append(role_item)
            elif isinstance(role_item, dict):
                if 'role' in role_item and isinstance(role_item['role'], dict):
                    role_name = role_item['role'].get('name')
                    if role_name:
                        roles.append(role_name)
                else:
                    role_name = role_item.get('name') or role_item.get('roleName')
                    if role_name and isinstance(role_name, str):
                        roles.append(role_name)
        
        print(f"[API] Parsed roles: {roles}")
        return roles
    
    def has_role(self, role_name):
        if not self.roles:
            return False
        for r in self.roles:
            if isinstance(r, str) and isinstance(role_name, str):
                if r == role_name or r.lower() == role_name.lower():
                    return True
        return False


class Package:
    def __init__(self, data):
        self.id = data.get('id')
        self.height = data.get('height', 0)
        self.width = data.get('width', 0)
        self.depth = data.get('depth', 0)
        self.weight = data.get('weight', 0)
        
        self.recipient_name = data.get('recipientName', '')
        self.tracking_number = data.get('trackingNumber', '')
        self.status = data.get('status', '')
        
        self.volume = self.height * self.width * self.depth
    
    def __str__(self):
        return f"Package(id={self.id}, {self.height}x{self.width}x{self.depth})"


class LockerInfo:
    def __init__(self, data):
        if isinstance(data, dict):
            self.id = data.get('id')
            self.number = data.get('number', self.id)
            self.status = data.get('status', 'unknown')
        elif isinstance(data, int):
            self.id = data
            self.number = data
            self.status = 'available'


def parse_validation_response(data):
    if not data:
        return None
    
    if isinstance(data, dict):
        if 'isValid' in data and not data['isValid']:
            print("[API] Invalid response: isValid=False")
            return None
        
        if 'success' in data and not data['success']:
            print("[API] Invalid response: success=False")
            return None
        
        if 'succeeded' in data and not data['succeeded']:
            print("[API] Invalid response: succeeded=False")
            return None
        
        actual_data = data.get('data') or data.get('value') or data
        
        return ValidationResponse(actual_data)
    
    return None


def parse_packages(data):
    if not data:
        return []
    
    packages_array = data
    
    if isinstance(data, dict):
        packages_array = data.get('data') or data.get('value') or data.get('packages') or []
    
    if not isinstance(packages_array, list):
        return []
    
    return [Package(pkg) for pkg in packages_array]


class LockerPackage:
    def __init__(self, data):
        if isinstance(data, dict):
            self.locker_id = data.get('lockerId') or data.get('id')
            self.package_id = data.get('packageId')
        else:
            self.locker_id = data
            self.package_id = None
    
    def __str__(self):
        return f"LockerPackage(locker={self.locker_id}, package={self.package_id})"


def parse_lockers(data):
    if not data:
        return []
    
    lockers_array = data
    
    if isinstance(data, dict):
        lockers_array = data.get('data') or data.get('value') or data.get('lockers') or []
    
    if not isinstance(lockers_array, list):
        return []
    
    result = []
    for item in lockers_array:
        if isinstance(item, int):
            result.append(LockerPackage(item))
        elif isinstance(item, dict):
            result.append(LockerPackage(item))
    
    return result


def safe_get(data, *keys, default=None):
    result = data
    for key in keys:
        if isinstance(result, dict):
            result = result.get(key)
            if result is None:
                return default
        else:
            return default
    return result if result is not None else default

