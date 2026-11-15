// Họ tên: Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 5: Dashboard Tổng quan - IDashboardService.cs
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IDashboardService
{
    /// Số liệu thống kê tổng quan cho trang Dashboard (hỗ trợ lọc tháng/năm cho chi phí).
    Task<DashboardViewModel> LayThongKeAsync(int? thang = null, int? nam = null);
}
