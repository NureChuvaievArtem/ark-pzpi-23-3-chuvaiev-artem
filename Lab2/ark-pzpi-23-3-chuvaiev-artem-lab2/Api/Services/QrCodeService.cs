using Api.Services.Interfaces;
using QRCoder;
using System.Drawing;

namespace Api.Services;

public class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrCodeImage(string data, int pixelsPerModule = 10)
    {
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new QRCode(qrCodeData);
        using var qrCodeImage = qrCode.GetGraphic(pixelsPerModule);
        
        using var ms = new MemoryStream();
        qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
        return ms.ToArray();
    }

    public string GenerateQrCodeDataUrl(string data, int pixelsPerModule = 10)
    {
        var imageBytes = GenerateQrCodeImage(data, pixelsPerModule);
        return $"data:image/png;base64,{Convert.ToBase64String(imageBytes)}";
    }
}

