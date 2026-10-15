//Interface Repository Thiết bị
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IThietBiRepository : IRepository<ThietBi>
{
    ///Tìm theo Tên / Số serial / Tên khu vực (LINQ). Từ khóa rỗng = lấy tất cả. Kèm Loại + Khu vực.
    Task<List<ThietBi>> TimKiemAsync(string? tuKhoa);

    Task<ThietBi?> LayChiTietAsync(int maThietBi);
    Task<bool> SoSerialDaTonTaiAsync(string soSerial, int? boQuaId = null);
    Task<DuLieuLienQuanThietBi> DemDuLieuLienQuanAsync(int maThietBi);

    Task<bool> LoaiDangHoatDongAsync(int maLoaiThietBi);
    Task<bool> KhuVucDangHoatDongAsync(int maKhuVuc);

    ///Loại đang sử dụng để chọn; giữ lại loại hiện tại của thiết bị dù đã ngừng.
    Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa);
    Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa);
}
