// Họ tên:  Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Interface Service Khu vực
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IKhuVucService
{
    Task<KhuVucIndexViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai);
    Task<KhuVuc?> LayTheoIdAsync(int id);
    Task<ServiceResult> ThemAsync(KhuVuc model);
    Task<ServiceResult> SuaAsync(KhuVuc model);

    /// <summary>Đổi Hoạt động &lt;-&gt; Ngừng hoạt động. Data = trạng thái mới.</summary>
    Task<ServiceResult<bool>> DoiTrangThaiAsync(int id);

    Task<ServiceResult> XoaAsync(int id);
}
