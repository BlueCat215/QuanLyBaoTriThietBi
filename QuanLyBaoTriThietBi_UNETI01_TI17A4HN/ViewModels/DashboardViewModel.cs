// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - ViewModel Dashboard (SST 44: 8 chỉ số thống kê tổng quan + danh sách cảnh báo nghiệp vụ)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// Một dòng số liệu của biểu đồ cột ngang: nhãn, số lượng, màu cột (class .bar-fill khớp màu badge), đường dẫn lọc.
public record ThongKeMuc(string Nhan, int SoLuong, string MauCot, string? Link = null);

/// Thiết bị quá hạn hoặc sắp đến hạn cần chú ý trên Dashboard.
public record ThietBiCanBaoTriItem(
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string? TenLoaiThietBi,
    string? TenKhuVuc,
    DateTime NgayBaoTriTiepTheo,
    TrangThaiHanBaoTriEnum TrangThaiHan,
    int SoNgayLech
);

/// Báo cáo sự cố mới gửi hoặc đã tiếp nhận cần xử lý gấp.
public record BaoCaoChoXuLyItem(
    int MaBaoCao,
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string MoTaSuCo,
    MucDoSuCoEnum MucDoSuCo,
    TrangThaiBaoCaoEnum TrangThai,
    DateTime NgayBaoCao,
    string? NguoiBao
);

public class DashboardViewModel
{
    /// true = số liệu đang là dữ liệu tạm (khung giao diện, việc 21); false khi đã nối số liệu thật (việc 44).
    public bool LaDuLieuMinhHoa { get; set; }

    // ---- 8 chỉ số theo yêu cầu (việc 44) ----

    /// 1. Tổng số thiết bị.
    public int TongThietBi { get; set; }

    /// 2. Số thiết bị theo tình trạng.
    public List<ThongKeMuc> ThietBiTheoTinhTrang { get; set; } = new();

    /// 3. Số thiết bị sắp đến hạn bảo trì.
    public int ThietBiSapDenHan { get; set; }

    /// 4. Số thiết bị quá hạn bảo trì.
    public int ThietBiQuaHan { get; set; }

    /// 5. Số kế hoạch bảo trì đang chờ thực hiện.
    public int KeHoachChoThucHien { get; set; }

    /// 6. Số báo cáo sự cố theo trạng thái.
    public List<ThongKeMuc> SuCoTheoTrangThai { get; set; } = new();

    /// 7. Số phiếu xử lý theo trạng thái.
    public List<ThongKeMuc> PhieuTheoTrangThai { get; set; } = new();

    /// 8. Tổng chi phí trong tháng (Thang/Nam là tháng được thống kê).
    public decimal TongChiPhiThang { get; set; }
    public int Thang { get; set; }
    public int Nam { get; set; }

    // ---- Danh sách hành động nhanh cho Quản trị viên ----

    /// Thiết bị quá hạn hoặc sắp đến hạn cần lập kế hoạch bảo trì
    public List<ThietBiCanBaoTriItem> ThietBiCanBaoTri { get; set; } = new();

    /// Sự cố mới / Đã tiếp nhận đang chờ lập phiếu xử lý
    public List<BaoCaoChoXuLyItem> SuCoMoiCanXuLy { get; set; } = new();

    /// Phần trăm của một mục so với tổng cả nhóm (0-100, số nguyên) để đặt độ rộng thanh; mục > 0 luôn hiện tối thiểu 1%.
    public static int PhanTram(ThongKeMuc muc, IReadOnlyCollection<ThongKeMuc> nhom)
    {
        var tong = nhom.Sum(m => m.SoLuong);
        if (tong <= 0 || muc.SoLuong <= 0)
            return 0;
        return Math.Max(1, (int)Math.Round(muc.SoLuong * 100.0 / tong));
    }
}
