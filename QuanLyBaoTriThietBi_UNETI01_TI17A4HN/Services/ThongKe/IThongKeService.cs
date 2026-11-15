// Họ tên: Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 5: Báo cáo Thống kê đa chiều - IThongKeService.cs
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IThongKeService
{
    /// Lấy báo cáo thống kê phân tích toàn diện theo bộ lọc thời gian
    Task<ThongKeIndexViewModel> LayThongKeAsync(ThongKeBoLoc boLoc);

    /// <summary>
    /// Xuất toàn bộ báo cáo phân tích thống kê đa chiều ra tệp Excel (.xlsx) chuẩn hóa
    /// </summary>
    Task<byte[]> XuatExcelAsync(ThongKeBoLoc boLoc, string? nguoiXuat = null);
}
