// Họ tên: Nguyễn Minh Đức
// Nội dung: Interface Service Thiết bị (Module 2: CRUD, SFSP, hạn bảo trì)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IThietBiService
{
    Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiBoLoc boLoc);
    Task<ThietBiDetailsViewModel?> LayChiTietAsync(int id);
    Task<ThietBiFormViewModel?> LayDeSuaAsync(int id);

    /// Đếm số thiết bị sắp đến hạn và quá hạn bảo trì (dùng chung cho Dashboard/Thống kê)
    Task<(int SapDenHan, int QuaHan)> DemHanBaoTriAsync(DateTime? homNay = null);

    /// Lấy danh sách thiết bị sắp đến hạn bảo trì
    Task<List<ThietBiRow>> LayDanhSachSapDenHanAsync(int top = 5, DateTime? homNay = null);

    /// Lấy danh sách thiết bị đã quá hạn bảo trì
    Task<List<ThietBiRow>> LayDanhSachQuaHanAsync(int top = 5, DateTime? homNay = null);

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
