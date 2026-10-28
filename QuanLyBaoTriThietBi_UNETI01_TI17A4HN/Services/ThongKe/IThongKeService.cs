// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Service Báo cáo thống kê (SST 45)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IThongKeService
{
    /// Lấy báo cáo thống kê phân tích toàn diện theo bộ lọc thời gian
    Task<ThongKeIndexViewModel> LayThongKeAsync(ThongKeBoLoc boLoc);
}
