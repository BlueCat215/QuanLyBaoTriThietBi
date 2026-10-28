// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Service Dashboard (SST 44)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IDashboardService
{
    /// Số liệu thống kê tổng quan cho trang Dashboard (hỗ trợ lọc tháng/năm cho chi phí).
    Task<DashboardViewModel> LayThongKeAsync(int? thang = null, int? nam = null);
}
