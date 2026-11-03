// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Nội dung: STT 56 - Helper sinh mã QR động (QRCodeHelper) sử dụng thư viện QRCoder
#nullable enable
using System.Text;
using QRCoder;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

public static class QRCodeHelper
{
    /// Sinh mã QR dạng chuỗi Data URI PNG (data:image/png;base64,...), có thể nhúng trực tiếp vào thẻ &lt;img src="..." /&gt;
    /// <param name="noiDung">Chuỗi nội dung cần mã hóa (URL, mã thiết bị, serial...)</param>
    /// <param name="pixelsPerModule">Kích thước mỗi module điểm ảnh (mặc định 6)</param>
    /// <returns>Chuỗi Data URI Base64 PNG</returns>
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

    /// Sinh mã QR dưới dạng mảng byte PNG (dùng cho API trả về file ảnh hoặc tải xuống)
    public static byte[] TaoBytesPng(string noiDung, int pixelsPerModule = 6)
    {
        if (string.IsNullOrWhiteSpace(noiDung))
            return Array.Empty<byte>();

        using var generator = new QRCodeGenerator();
        using var qrCodeData = generator.CreateQrCode(noiDung, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);
        return pngQrCode.GetGraphic(pixelsPerModule);
    }

    /// Sinh mã QR dạng thẻ vector SVG
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
