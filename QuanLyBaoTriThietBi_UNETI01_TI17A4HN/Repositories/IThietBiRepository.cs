//Interface Repository Thiết bị
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IThietBiRepository : IRepository<ThietBi>
{
    ///Câu truy vấn đã Tìm kiếm + Lọc + Sắp xếp (CHƯA thực thi, kèm Loại + Khu vực).
    IQueryable<ThietBi> TruyVan(ThietBiBoLoc boLoc);

    ///Tìm theo Tên / Số serial / Tên khu vực, Lọc, Sắp xếp rồi lấy toàn bộ kết quả.
    Task<List<ThietBi>> TimKiemAsync(ThietBiBoLoc boLoc);

    Task<ThietBi?> LayChiTietAsync(int maThietBi);
    Task<bool> SoSerialDaTonTaiAsync(string soSerial, int? boQuaId = null);
    Task<DuLieuLienQuanThietBi> DemDuLieuLienQuanAsync(int maThietBi);

    Task<bool> LoaiDangHoatDongAsync(int maLoaiThietBi);
    Task<bool> KhuVucDangHoatDongAsync(int maKhuVuc);

    ///Loại đang sử dụng để chọn; giữ lại loại hiện tại của thiết bị dù đã ngừng.
    Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa);
    Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa);

    ///Toàn bộ loại / khu vực (kể cả đã ngừng) cho ô Lọc, vì thiết bị cũ vẫn có thể thuộc loại/khu vực đó.
    Task<List<(int Ma, string Ten)>> LayLoaiDeLocAsync();
    Task<List<(int Ma, string Ten)>> LayKhuVucDeLocAsync();
}
