// Họ tên: <điền họ tên>
// MSSV: <điền MSSV>
// Nội dung: Module 1 - Interface Service Tài khoản
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

/// <remarks>maNguoiThucHien = MaTaiKhoan đang đăng nhập (null nếu chưa có đăng nhập) để chặn tự khóa/tự xóa chính mình.</remarks>
public interface ITaiKhoanService
{
    Task<TaiKhoanIndexViewModel> LayDanhSachAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai, int? maNguoiThucHien);
    Task<TaiKhoanFormViewModel?> LayDeSuaAsync(int id);
    Task<List<KhuVucOption>> LayKhuVucChoChonAsync(int? giuLaiMa);

    Task<ServiceResult> ThemAsync(TaiKhoanFormViewModel vm);
    Task<ServiceResult> SuaAsync(TaiKhoanFormViewModel vm, int? maNguoiThucHien);

    /// <summary>Khóa &lt;-&gt; Mở khóa. Data = trạng thái mới (true = đang hoạt động).</summary>
    Task<ServiceResult<bool>> DoiTrangThaiAsync(int id, int? maNguoiThucHien);

    Task<ServiceResult> XoaAsync(int id, int? maNguoiThucHien);
}
