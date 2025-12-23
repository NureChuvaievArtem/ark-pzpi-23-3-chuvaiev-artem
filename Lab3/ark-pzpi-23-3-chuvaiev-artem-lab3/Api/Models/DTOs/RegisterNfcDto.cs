namespace Api.Models.DTOs;

public class RegisterNfcDto
{
    public int UserId { get; set; }
    public string SerialNumber { get; set; }
    public string? QrCode { get; set; }
}

