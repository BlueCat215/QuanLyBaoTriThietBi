// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - ViewModel Báo cáo thống kê (SST 45: LINQ GroupBy theo loại, khu vực, mức độ, KTV, chi phí & hiệu suất)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// Bộ lọc khoảng thời gian hoặc theo năm cho trang Thống kê
public class ThongKeBoLoc
{
    public int? Nam { get; set; } = DateTime.Today.Year;
    public DateTime? TuNgay { get; set; }
    public DateTime? DenNgay { get; set; }

    public bool CoBoLocKhoangNgay => TuNgay.HasValue || DenNgay.HasValue;
    public bool KhoangNgayKhongHopLe => TuNgay.HasValue && DenNgay.HasValue && TuNgay.Value.Date > DenNgay.Value.Date;
}

/// Dòng thống kê nhóm theo Loại thiết bị hoặc theo Khu vực
public record ThongKeNhomItem(
    int MaNhom,
    string TenNhom,
    int SoThietBi,
    int SoSuCo,
    int SoKeHoach,
    int SoPhieuXuLy,
    decimal TongChiPhi
);

/// Thống kê theo mức độ sự cố
public record ThongKeMucDoItem(
    MucDoSuCoEnum MucDo,
    string TenMucDo,
    int SoLuong,
    int SoLuongDaXong,
    double TyLePhanTram
);

/// Thống kê hiệu suất làm việc theo kỹ thuật viên
public record ThongKeKtvItem(
    int MaKyThuatVien,
    string TenKyThuatVien,
    string? ChuyenMon,
    string? SoDienThoai,
    int TongPhieuPhuTrach,
    int SoPhieuHoanThanh,
    int SoPhieuDangXuLy,
    double ThoiGianXuLyTrungBinhGio,
    decimal TongChiPhiVatTu
);

/// Thiết bị có nhiều sự cố nhất (cần bảo dưỡng hoặc thay thế)
public record ThietBiNhieuSuCoItem(
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string? TenLoaiThietBi,
    string? TenKhuVuc,
    int SoSuCo,
    int SoPhieuBaoTri,
    decimal TongChiPhi,
    TinhTrangThietBiEnum TinhTrang
);

/// Thiết bị phát sinh chi phí bảo trì & sửa chữa cao nhất
public record ThietBiTonChiPhiItem(
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string? TenLoaiThietBi,
    string? TenKhuVuc,
    int SoPhieuXuLy,
    int SoLanDungVatTu,
    decimal TongChiPhi
);

/// Thống kê tổng chi phí theo từng tháng trong năm
public record ChiPhiTheoThangItem(
    int Thang,
    int Nam,
    int SoPhieuPhatSinh,
    decimal TongChiPhi
);

/// ViewModel chính chứa toàn bộ báo cáo phân tích theo yêu cầu mục 9.8
public class ThongKeIndexViewModel
{
    public ThongKeBoLoc BoLoc { get; set; } = new();

    // 1. Chỉ số tổng quan theo bộ lọc
    public int TongThietBi { get; set; }
    public int TongSuCo { get; set; }
    public int TongKeHoach { get; set; }
    public int TongPhieuXuLy { get; set; }
    public int TongPhieuHoanThanh { get; set; }
    public decimal TongChiPhi { get; set; }
    public double ThoiGianXuLyTbGio { get; set; }

    // 2. Thống kê GroupBy theo Loại thiết bị
    public List<ThongKeNhomItem> TheoLoaiThietBi { get; set; } = new();

    // 3. Thống kê GroupBy theo Khu vực
    public List<ThongKeNhomItem> TheoKhuVuc { get; set; } = new();

    // 4. Thống kê GroupBy theo Mức độ sự cố
    public List<ThongKeMucDoItem> TheoMucDoSuCo { get; set; } = new();

    // 5. Thống kê GroupBy theo Kỹ thuật viên
    public List<ThongKeKtvItem> TheoKyThuatVien { get; set; } = new();

    // 6. Top thiết bị có nhiều sự cố nhất
    public List<ThietBiNhieuSuCoItem> TopThietBiNhieuSuCo { get; set; } = new();

    // 7. Top KTV hoàn thành nhiều phiếu nhất
    public List<ThongKeKtvItem> TopKtvHoanThanhNhieuNhat { get; set; } = new();

    // 8. Top thiết bị tốn chi phí sửa chữa / bảo trì nhất
    public List<ThietBiTonChiPhiItem> TopThietBiTonChiPhi { get; set; } = new();

    // 9. Thống kê chi phí theo từng tháng của năm được chọn
    public List<ChiPhiTheoThangItem> ChiPhi12Thang { get; set; } = new();
}
