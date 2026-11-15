// Họ tên: Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 5: Dashboard Tổng quan - IDashboardRepository.cs
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

/// <summary>
/// DTO chứa thông tin cần thiết của thiết bị phục vụ tính hạn và thống kê trên Dashboard.
/// </summary>
public record ThietBiDashboardDto(
    int MaThietBi,
    string TenThietBi,
    string SoSerial,
    string? TenLoaiThietBi,
    string? TenKhuVuc,
    TinhTrangThietBiEnum TinhTrang,
    DateTime? NgayBaoTriGanNhat,
    DateTime NgayDuaVaoSuDung,
    int ChuKyBaoTriThang
);

public interface IDashboardRepository
{
    Task<List<ThietBiDashboardDto>> LayDanhSachThietBiAsync();
    Task<int> DemKeHoachChoThucHienAsync();
    Task<Dictionary<TrangThaiBaoCaoEnum, int>> DemBaoCaoSuCoTheoTrangThaiAsync();
    Task<List<BaoCaoChoXuLyItem>> LayBaoCaoChoXuLyAsync(int top = 5);
    Task<Dictionary<TrangThaiPhieuXuLyEnum, int>> DemPhieuXuLyTheoTrangThaiAsync();
    Task<decimal> TinhTongChiPhiThangAsync(int thang, int nam);
}
