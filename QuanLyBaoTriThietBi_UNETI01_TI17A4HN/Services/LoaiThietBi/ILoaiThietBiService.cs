// Họ tên: Nguyễn Văn Hoàng - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 1: Quản lý hệ thống & Danh mục - ILoaiThietBiService
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface ILoaiThietBiService
{
    Task<LoaiThietBiIndexViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai);
    Task<LoaiThietBi?> LayTheoIdAsync(int id);
    Task<ServiceResult> ThemAsync(LoaiThietBi model);
    Task<ServiceResult> SuaAsync(LoaiThietBi model);

    /// <summary>Đổi Hoạt động &lt;-&gt; Ngừng sử dụng. Data = trạng thái mới.</summary>
    Task<ServiceResult<bool>> DoiTrangThaiAsync(int id);

    Task<ServiceResult> XoaAsync(int id);
}
