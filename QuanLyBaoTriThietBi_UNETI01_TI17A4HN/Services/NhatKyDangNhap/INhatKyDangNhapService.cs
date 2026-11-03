// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Interface Service Nhật ký đăng nhập (Audit Log)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface INhatKyDangNhapService
{
    /// <summary>
    /// Lấy dữ liệu phân trang, áp dụng bộ lọc và tính toán thống kê KPI cho trang Index.
    /// </summary>
    Task<NhatKyDangNhapIndexViewModel> LayDanhSachAsync(NhatKyDangNhapBoLoc boLoc);

    /// <summary>
    /// Xuất danh sách nhật ký đăng nhập theo bộ lọc ra file Excel chuẩn hóa.
    /// </summary>
    Task<byte[]> XuatExcelAsync(NhatKyDangNhapBoLoc boLoc, string nguoiXuat);
}
