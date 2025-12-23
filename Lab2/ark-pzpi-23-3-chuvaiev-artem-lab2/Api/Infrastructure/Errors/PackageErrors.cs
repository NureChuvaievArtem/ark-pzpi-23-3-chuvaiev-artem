namespace Api.Infrastructure.Errors;

public static class PackageErrors
{
    public static Error PackageNotFound()
    {
        return Error.NotFound("package.NOT_FOUND", "Package not found");
    }
    
    public static Error InvalidStatusTransition()
    {
        return Error.Validation("package.INVALID_STATUS_TRANSITION", "Invalid status transition");
    }
    
    public static Error LockerAlreadyOccupied()
    {
        return Error.Conflict("package.LOCKER_OCCUPIED", "Locker is already occupied");
    }
    
    public static Error LockerNotFound()
    {
        return Error.NotFound("package.LOCKER_NOT_FOUND", "Locker not found");
    }
    
    public static Error LockerNotBoundToClient()
    {
        return Error.Forbidden("package.LOCKER_NOT_BOUND", "Locker is not bound to this client");
    }
}

