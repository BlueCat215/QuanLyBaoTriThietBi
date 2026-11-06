// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IThietBiRepository : IRepository<ThietBi>
{
    IQueryable<ThietBi> TruyVan(ThietBiBoLoc boLoc);

    Task<List<ThietBi>> TimKiemAsync(ThietBiBoLoc boLoc);

    Task<PagedResult<ThietBi>> LayPhanTrangAsync(ThietBiBoLoc boLoc);

    Task<ThietBi?> LayChiTietAsync(int maThietBi);

    Task<bool> SoSerialDaTonTaiAsync(string soSerial, int? boQuaId = null);

    Task<DuLieuLienQuanThietBi> DemDuLieuLienQuanAsync(int maThietBi);

    Task<int> DemSapDenHanAsync(DateTime? homNay = null);

    Task<int> DemQuaHanAsync(DateTime? homNay = null);

    Task<List<ThietBi>> LayDanhSachSapDenHanAsync(int top = 5, DateTime? homNay = null);

    Task<List<ThietBi>> LayDanhSachQuaHanAsync(int top = 5, DateTime? homNay = null);

    Task<bool> LoaiDangHoatDongAsync(int maLoaiThietBi);

    Task<bool> KhuVucDangHoatDongAsync(int maKhuVuc);

    Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa);

    Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa);

    Task<List<(int Ma, string Ten)>> LayLoaiDeLocAsync();

    Task<List<(int Ma, string Ten)>> LayKhuVucDeLocAsync();
}
