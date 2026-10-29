// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - ViewModel Lịch sử thiết bị (STT 41: Kế hoạch bảo trì, sự cố, phiếu xử lý, KTV đã phụ trách, ngày hoàn thành, chi phí lũy kế)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public record ThietBiThongTinLichSu(
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string? TenLoaiThietBi,
    string? TenKhuVuc,
    TinhTrangThietBiEnum TinhTrang,
    DateTime NgayDuaVaoSuDung,
    int ChuKyBaoTriThang,
    DateTime? NgayBaoTriGanNhat,
    DateTime NgayBaoTriTiepTheo,
    string? MoTa
);

public record LichSuKtvPhanCong(
    int MaPhanCong,
    int MaKyThuatVien,
    string TenKyThuatVien,
    string? SoDienThoai,
    string? Email,
    string? ChuyenMon,
    DateTime NgayPhanCong,
    DateTime? NgayKetThucPhanCong,
    string? NoiDungPhanCong,
    TrangThaiPhanCongEnum TrangThai,
    bool LaHienTai
);

public record LichSuPhieuXuLyItem(
    int MaPhieuXuLy,
    LoaiXuLyEnum LoaiXuLy,
    int? MaBaoCao,
    int? MaKeHoach,
    string NguonText,
    DateTime NgayTao,
    DateTime? NgayBatDau,
    DateTime? NgayHoanThanh,
    TrangThaiPhieuXuLyEnum TrangThai,
    string? KetQuaXuLy,
    decimal TongChiPhi,
    List<LichSuKtvPhanCong> DanhSachKtv,
    List<ChiTietXuLyRow> DanhSachChiTiet
);

public record LichSuKeHoachItem(
    int MaKeHoach,
    DateTime NgayDuKien,
    string NoiDungBaoTri,
    MucDoUuTienEnum MucDoUuTien,
    TrangThaiKeHoachEnum TrangThai,
    string? GhiChu,
    int? MaPhieuXuLy,
    TrangThaiPhieuXuLyEnum? TrangThaiPhieu,
    string? TenKtvPhuTrach,
    DateTime? NgayHoanThanh,
    decimal ChiPhi
);

public record LichSuSuCoItem(
    int MaBaoCao,
    DateTime NgayBaoCao,
    string NguoiBaoCao,
    string MoTaSuCo,
    MucDoSuCoEnum MucDoSuCo,
    TrangThaiBaoCaoEnum TrangThai,
    string? GhiChu,
    int? MaPhieuXuLy,
    TrangThaiPhieuXuLyEnum? TrangThaiPhieu,
    string? TenKtvPhuTrach,
    DateTime? NgayHoanThanh,
    decimal ChiPhi
);

public record TimelineEventItem(
    DateTime Ngay,
    string LoaiSuKien,
    string CssClass, // is-maintenance, is-incident, is-done, is-reassign
    string TieuDe,
    string ChiTiet,
    string? NguoiThucHien,
    decimal? ChiPhi,
    string? UrlChiTiet,
    string? BadgeText,
    string? BadgeClass
);

public class LichSuThietBiViewModel
{
    public ThietBiThongTinLichSu ThietBi { get; set; } = null!;

    // Chỉ số tóm tắt (KPIs)
    public int TongSoKeHoach { get; set; }
    public int TongSoSuCo { get; set; }
    public int TongSoPhieuXuLy { get; set; }
    public int SoPhieuHoanThanh { get; set; }
    public decimal TongChiPhiLuyKe { get; set; }

    // Dữ liệu chi tiết từng phần
    public List<LichSuKeHoachItem> KeHoachBaoTris { get; set; } = new();
    public List<LichSuSuCoItem> BaoCaoSuCos { get; set; } = new();
    public List<LichSuPhieuXuLyItem> PhieuXuLys { get; set; } = new();
    public List<TimelineEventItem> TimelineEvents { get; set; } = new();
}
