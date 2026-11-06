// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị - QRCodeHelper
#nullable enable
using QRCoder;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

public static class QRCodeHelper
{
    public static string TaoDataUriPng(string noiDung, int pixelsPerModule = 6)
    {
        if (string.IsNullOrWhiteSpace(noiDung))
            return string.Empty;

        using var generator = new QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(noiDung, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = pngQrCode.GetGraphic(pixelsPerModule);
        var base64 = Convert.ToBase64String(qrCodeBytes);
        return $"data:image/png;base64,{base64}";
    }

    public static byte[] TaoBytesPng(string noiDung, int pixelsPerModule = 6)
    {
        if (string.IsNullOrWhiteSpace(noiDung))
            return Array.Empty<byte>();

        using var generator = new QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(noiDung, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);
        return pngQrCode.GetGraphic(pixelsPerModule);
    }

    public static string TaoSvg(string noiDung, int pixelsPerModule = 6)
    {
        if (string.IsNullOrWhiteSpace(noiDung))
            return string.Empty;

        using var generator = new QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(noiDung, QRCodeGenerator.ECCLevel.Q);
        var svgQrCode = new SvgQRCode(qrCodeData);
        return svgQrCode.GetGraphic(pixelsPerModule);
    }
}
