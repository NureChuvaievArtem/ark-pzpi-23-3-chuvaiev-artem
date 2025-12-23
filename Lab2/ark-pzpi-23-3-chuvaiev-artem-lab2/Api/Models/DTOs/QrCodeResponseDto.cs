namespace Api.Models.DTOs;

public class QrCodeResponseDto
{
    public string QrCode { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
}

