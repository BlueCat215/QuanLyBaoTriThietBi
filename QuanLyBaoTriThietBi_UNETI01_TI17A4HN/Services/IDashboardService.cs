// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Service Dashboard
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IDashboardService
{
    /// Số liệu thống kê tổng quan cho trang Dashboard.
    Task<DashboardViewModel> LayThongKeAsync();
}
