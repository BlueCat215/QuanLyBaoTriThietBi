// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị - IThietBiService
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IThietBiService
{
    Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiBoLoc boLoc);

    Task<List<ThietBiRow>> LayToanBoDeXuatExcelAsync(ThietBiBoLoc boLoc);

    Task<ThietBiDetailsViewModel?> LayChiTietAsync(int id);

    Task<ThietBiFormViewModel?> LayDeSuaAsync(int id);

    Task<(int SapDenHan, int QuaHan)> DemHanBaoTriAsync(DateTime? homNay = null);

    Task<List<ThietBiRow>> LayDanhSachSapDenHanAsync(int top = 5, DateTime? homNay = null);

    Task<List<ThietBiRow>> LayDanhSachQuaHanAsync(int top = 5, DateTime? homNay = null);

    Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa);

    Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa);

    Task<ServiceResult<int>> ThemAsync(ThietBiFormViewModel vm, int? maTaiKhoan = null);

    Task<ServiceResult> SuaAsync(ThietBiFormViewModel vm, int? maTaiKhoan = null);

    Task<ServiceResult<TinhTrangThietBiEnum>> DoiTinhTrangAsync(
        int id,
        TinhTrangThietBiEnum tinhTrangMoi
    );

    Task<ServiceResult> XoaAsync(int id);
}
