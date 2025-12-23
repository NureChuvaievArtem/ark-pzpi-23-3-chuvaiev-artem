namespace Api.Infrastructure.Errors;

public static class NfcErrors
{
    public static Error NfcNotFound()
    {
        return Error.NotFound("nfc.NOT_FOUND", "NFC card not found");
    }
    
    public static Error NfcAlreadyRegistered()
    {
        return Error.Conflict("nfc.ALREADY_REGISTERED", "NFC card is already registered");
    }
    
    public static Error InvalidQrCode()
    {
        return Error.Validation("nfc.INVALID_QR_CODE", "Invalid or expired QR code");
    }
    
    public static Error UnauthorizedAccess()
    {
        return Error.Unauthorized("nfc.UNAUTHORIZED", "Unauthorized NFC card access");
    }
}

