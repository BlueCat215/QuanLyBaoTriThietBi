#nullable enable
using System.Globalization;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

/// <summary>
/// Thông tin hiển thị của một badge (nhãn).
/// Kind: "status" (trạng thái dạng bo tròn), "level" (mức độ sự cố), "priority" (mức độ ưu tiên), "role" (vai trò).
/// </summary>
public readonly record struct BadgeInfo(string Text, string Css, string Kind = "status");

/// <summary>
/// NƠI DUY NHẤT quy định: nhãn tiếng Việt + màu badge của mỗi enum, và định dạng tiền/ngày.
/// Không tự viết "if (TrangThai == ...) class=..." trong View - hãy gọi UiHelper.Badge(...).
/// Bảng màu đầy đủ: docs/GIAO_DIEN.md mục 4.
/// </summary>
public static class UiHelper
{
    /// <summary>Thiết bị còn bao nhiêu ngày thì tính là "Sắp đến hạn".</summary>
    public const int NgayCanhBaoSapDenHan = 7;

    // ĐỊNH DẠNG
    
    private static readonly NumberFormatInfo SoViet = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ","
    };

    /// <summary>1250000 -> "1.250.000 ₫"</summary>
    public static string Tien(decimal? value) => value is null ? "—" : value.Value.ToString("N0", SoViet) + " ₫";

    /// <summary>dd/MM/yyyy</summary>
    public static string Ngay(DateTime? value) => value?.ToString("dd/MM/yyyy") ?? "—";

    /// <summary>dd/MM/yyyy HH:mm</summary>
    public static string NgayGio(DateTime? value) => value?.ToString("dd/MM/yyyy HH:mm") ?? "—";

    /// <summary>Mã hiển thị: Ma("PX", 7) -> "PX-0007" (PX = Phiếu xử lý, KH = Kế hoạch, SC = Sự cố, TB = Thiết bị).</summary>
    public static string Ma(string tienTo, int id) => $"{tienTo}-{id:D4}";

    /// <summary>Chữ viết tắt cho avatar: "Nguyễn Quản Trị" -> "QT" (chữ cái đầu của 2 từ cuối).</summary>
    public static string VietTat(string? hoTen)
    {
        var tu = (hoTen ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tu.Length == 0) return "?";
        if (tu.Length == 1) return char.ToUpperInvariant(tu[0][0]).ToString();
        return string.Concat(char.ToUpperInvariant(tu[^2][0]), char.ToUpperInvariant(tu[^1][0]));
    }

    // HẠN BẢO TRÌ (tính động, không lưu DB)
    
    /// <summary>
    /// Badge "Chưa đến hạn / Sắp đến hạn / Quá hạn" + dòng ghi chú nhỏ ("còn 5 ngày" / "quá 12 ngày").
    /// </summary>
    public static (BadgeInfo Badge, string Note) HanBaoTri(DateTime ngayBaoTriTiepTheo, DateTime? homNay = null)
    {
        var soNgay = (ngayBaoTriTiepTheo.Date - (homNay ?? DateTime.Today).Date).Days;
        
        if (soNgay < 0) 
            return (new BadgeInfo("Quá hạn", "is-danger"), $"quá {-soNgay} ngày");
            
        if (soNgay <= NgayCanhBaoSapDenHan) 
            return (new BadgeInfo("Sắp đến hạn", "is-warning"), soNgay == 0 ? "hôm nay" : $"còn {soNgay} ngày");
            
        return (new BadgeInfo("Chưa đến hạn", "is-success"), $"còn {soNgay} ngày");
    }

    // BADGE THEO ENUM
    
    public static BadgeInfo Badge(TinhTrangThietBiEnum v) => v switch
    {
        TinhTrangThietBiEnum.DangHoatDong => new("Đang hoạt động", "is-success"),
        TinhTrangThietBiEnum.DangBaoTri => new("Đang bảo trì", "is-info"),
        TinhTrangThietBiEnum.TamNgungDoSuCo => new("Tạm ngừng do sự cố", "is-warning is-strong"),
        TinhTrangThietBiEnum.NgungSuDung => new("Ngừng sử dụng", "is-muted"),
        _ => new(v.ToString(), "is-muted")
    };

    public static BadgeInfo Badge(TrangThaiKeHoachEnum v) => v switch
    {
        TrangThaiKeHoachEnum.ChoThucHien => new("Chờ thực hiện", "is-warning"),
        TrangThaiKeHoachEnum.DaPhanCong => new("Đã phân công", "is-info"),
        TrangThaiKeHoachEnum.DangThucHien => new("Đang thực hiện", "is-info is-strong"),
        TrangThaiKeHoachEnum.ChoNghiemThu => new("Chờ nghiệm thu", "is-purple"),
        TrangThaiKeHoachEnum.HoanThanh => new("Hoàn thành", "is-success"),
        TrangThaiKeHoachEnum.DaHuy => new("Đã hủy", "is-muted"),
        _ => new(v.ToString(), "is-muted")
    };

    public static BadgeInfo Badge(TrangThaiBaoCaoEnum v) => v switch
    {
        TrangThaiBaoCaoEnum.MoiBao => new("Mới báo", "is-warning has-alert-dot"),
        TrangThaiBaoCaoEnum.DaTiepNhan => new("Đã tiếp nhận", "is-info"),
        TrangThaiBaoCaoEnum.DangXuLy => new("Đang xử lý", "is-info is-strong"),
        TrangThaiBaoCaoEnum.ChoNghiemThu => new("Chờ nghiệm thu", "is-purple"),
        TrangThaiBaoCaoEnum.HoanThanh => new("Hoàn thành", "is-success"),
        TrangThaiBaoCaoEnum.DaHuy => new("Đã hủy", "is-muted"),
        _ => new(v.ToString(), "is-muted")
    };

    public static BadgeInfo Badge(TrangThaiPhieuXuLyEnum v) => v switch
    {
        TrangThaiPhieuXuLyEnum.ChoPhanCong => new("Chờ phân công", "is-warning"),
        TrangThaiPhieuXuLyEnum.DaPhanCong => new("Đã phân công", "is-info"),
        TrangThaiPhieuXuLyEnum.DangXuLy => new("Đang xử lý", "is-info is-strong"),
        TrangThaiPhieuXuLyEnum.ChoNghiemThu => new("Chờ nghiệm thu", "is-purple"),
        TrangThaiPhieuXuLyEnum.HoanThanh => new("Hoàn thành", "is-success"),
        TrangThaiPhieuXuLyEnum.DaHuy => new("Đã hủy", "is-muted"),
        _ => new(v.ToString(), "is-muted")
    };

    public static BadgeInfo Badge(TrangThaiPhanCongEnum v) => v switch
    {
        TrangThaiPhanCongEnum.DangHieuLuc => new("Đang hiệu lực", "is-success"),
        TrangThaiPhanCongEnum.DaKetThuc => new("Đã kết thúc", "is-muted"),
        _ => new(v.ToString(), "is-muted")
    };

    public static BadgeInfo Badge(MucDoSuCoEnum v) => v switch
    {
        MucDoSuCoEnum.Thap => new("Thấp", "is-thap", "level"),
        MucDoSuCoEnum.TrungBinh => new("Trung bình", "is-trungbinh", "level"),
        MucDoSuCoEnum.Cao => new("Cao", "is-cao", "level"),
        MucDoSuCoEnum.KhanCap => new("Khẩn cấp", "is-khancap", "level"),
        _ => new(v.ToString(), "is-thap", "level")
    };

    public static BadgeInfo Badge(MucDoUuTienEnum v) => v switch
    {
        MucDoUuTienEnum.Thap => new("Thấp", "is-thap", "priority"),
        MucDoUuTienEnum.TrungBinh => new("Trung bình", "is-trungbinh", "priority"),
        MucDoUuTienEnum.Cao => new("Cao", "is-cao", "priority"),
        _ => new(v.ToString(), "is-thap", "priority")
    };

    public static BadgeInfo Badge(VaiTroEnum v) => v switch
    {
        VaiTroEnum.Admin => new("Quản trị viên", "is-admin", "role"),
        VaiTroEnum.KyThuatVien => new("Kỹ thuật viên", "is-ktv", "role"),
        VaiTroEnum.NguoiSuDung => new("Người sử dụng", "is-user", "role"),
        _ => new(v.ToString(), "is-user", "role")
    };

    public static BadgeInfo Badge(KetQuaDangNhapEnum v) => v switch
    {
        KetQuaDangNhapEnum.ThanhCong => new("Thành công", "is-success"),
        KetQuaDangNhapEnum.SaiMatKhau => new("Sai mật khẩu", "is-warning"),
        KetQuaDangNhapEnum.TaiKhoanBiKhoa => new("Tài khoản bị khóa", "is-danger"),
        KetQuaDangNhapEnum.KhongTonTaiTaiKhoan => new("Không tồn tại", "is-muted"),
        _ => new(v.ToString(), "is-muted")
    };

    /// <summary>Cột TrangThai kiểu bit. laTaiKhoan = true thì false hiện "Bị khóa" (đỏ), ngược lại "Ngừng hoạt động" (xám).</summary>
    public static BadgeInfo BadgeHoatDong(bool hoatDong, bool laTaiKhoan = false) => 
        hoatDong 
            ? new BadgeInfo("Hoạt động", "is-success") 
            : laTaiKhoan 
                ? new BadgeInfo("Bị khóa", "is-danger") 
                : new BadgeInfo("Ngừng hoạt động", "is-muted");
}
