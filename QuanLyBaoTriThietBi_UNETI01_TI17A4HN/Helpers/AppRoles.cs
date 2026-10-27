#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

/// <summary>
/// Hằng số định danh vai trò người dùng trong hệ thống.
/// Sử dụng trực tiếp với [Authorize(Roles = AppRoles.Admin)] để tránh gõ sai chính tả (magic strings).
/// </summary>
public static class AppRoles
{
    public const string Admin = nameof(VaiTroEnum.Admin);
    public const string KyThuatVien = nameof(VaiTroEnum.KyThuatVien);
    public const string NguoiSuDung = nameof(VaiTroEnum.NguoiSuDung);

    /// <summary>
    /// Cho phép cả Admin và Kỹ thuật viên (thường dùng cho nghiệp vụ bảo trì, xử lý thiết bị)
    /// </summary>
    public const string AdminOrKyThuatVien = $"{Admin},{KyThuatVien}";

    /// <summary>
    /// Tất cả các vai trò đã đăng nhập
    /// </summary>
    public const string All = $"{Admin},{KyThuatVien},{NguoiSuDung}";
}
