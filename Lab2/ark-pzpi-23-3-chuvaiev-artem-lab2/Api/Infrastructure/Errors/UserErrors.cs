namespace Api.Infrastructure.Errors;

public static class UserErrors
{
    public static Error UserNotFoundError()
    {
        return Error.NotFound("auth.USER_NOT_FOUND", "User not found");
    }
    
    public static Error Unauthorized()
    {
        return Error.Unauthorized("auth.UNAUTHORIZED", "User is not authorized");
    }
}