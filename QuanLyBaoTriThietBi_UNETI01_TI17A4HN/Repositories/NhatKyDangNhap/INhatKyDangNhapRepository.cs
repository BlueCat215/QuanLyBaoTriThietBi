// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Interface Repository Nhật ký đăng nhập (chỉ truy vấn, không xóa)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface INhatKyDangNhapRepository : IRepository<NhatKyDangNhap>
{
    /// <summary>
    /// Lấy danh sách nhật ký đăng nhập có phân trang và áp dụng bộ lọc.
    /// Sắp xếp theo thời gian mới nhất lên đầu.
    /// </summary>
    Task<PagedResult<NhatKyDangNhapRow>> LayDanhSachPagedAsync(NhatKyDangNhapBoLoc boLoc);

    /// <summary>
    /// Lấy toàn bộ danh sách phù hợp bộ lọc phục vụ xuất báo cáo Excel.
    /// </summary>
    Task<List<NhatKyDangNhapRow>> LayTatCaChoXuatExcelAsync(NhatKyDangNhapBoLoc boLoc);

    /// <summary>
    /// Thống kê tổng quan các chỉ số đăng nhập phục vụ KPI Dashboard.
    /// </summary>
    Task<NhatKyDangNhapThongKe> LayThongKeAsync();
}
