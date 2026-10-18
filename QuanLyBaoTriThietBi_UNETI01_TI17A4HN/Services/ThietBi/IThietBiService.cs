//Interface Service Thiết bị
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IThietBiService
{
    Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiBoLoc boLoc);
    Task<ThietBiDetailsViewModel?> LayChiTietAsync(int id);
    Task<ThietBiFormViewModel?> LayDeSuaAsync(int id);

    Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa);
    Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa);

    Task<ServiceResult> ThemAsync(ThietBiFormViewModel vm);
    Task<ServiceResult> SuaAsync(ThietBiFormViewModel vm);

    ///Đổi tình trạng (kể cả chuyển sang Ngừng sử dụng). Data = tình trạng mới.
    Task<ServiceResult<TinhTrangThietBiEnum>> DoiTinhTrangAsync(
        int id,
        TinhTrangThietBiEnum tinhTrangMoi
    );

    ///Chỉ xóa được thiết bị chưa phát sinh dữ liệu; còn lại phải chuyển Ngừng sử dụng.
    Task<ServiceResult> XoaAsync(int id);
}
