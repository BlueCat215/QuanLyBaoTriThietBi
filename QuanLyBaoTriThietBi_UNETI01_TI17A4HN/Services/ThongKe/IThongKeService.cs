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

    /// <summary>
    /// STT 61: Xuất toàn bộ báo cáo phân tích thống kê đa chiều ra tệp Excel (.xlsx) chuẩn hóa
    /// </summary>
    Task<byte[]> XuatExcelAsync(ThongKeBoLoc boLoc, string? nguoiXuat = null);
}
