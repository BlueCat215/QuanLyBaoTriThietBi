// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý Hệ thống & Danh mục dùng chung - IKhuVucService.cs
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
