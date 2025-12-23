namespace Api.Services.Interfaces;

public interface IQrCodeService
{
    byte[] GenerateQrCodeImage(string data, int pixelsPerModule = 10);
    string GenerateQrCodeDataUrl(string data, int pixelsPerModule = 10);
}

