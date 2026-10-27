// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Interface Repository Kế hoạch bảo trì
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IKeHoachBaoTriRepository : IRepository<KeHoachBaoTri>
{
    /// Tìm theo tên/serial thiết bị, tên khu vực, nội dung bảo trì + lọc trạng thái, mức ưu tiên.
    Task<List<KeHoachRow>> TimKiemAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    );

    Task<KeHoachBaoTri?> LayChiTietTheoIdAsync(int id);

    Task<ThietBi?> LayThietBiAsync(int maThietBi);

    Task<ThietBi?> LayThietBiCoTheoDoiAsync(int maThietBi);

    /// Thiết bị chưa Ngừng sử dụng (luôn giữ lại giuLaiMa để hiển thị khi sửa).
    Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(int? giuLaiMa);

    /// Business rule chống trùng (LINQ): kế hoạch ĐANG HOẠT ĐỘNG (không phải Hoàn thành/Đã hủy)
    /// của cùng thiết bị và cùng kỳ bảo trì (cùng tháng-năm của NgayDuKien). boQuaMaKeHoach dùng khi sửa.
    Task<KeHoachBaoTri?> TimKeHoachTrungKyAsync(
        int maThietBi,
        DateTime ngayDuKien,
        int? boQuaMaKeHoach = null
    );

    /// Số phiếu xử lý chưa bị hủy của kế hoạch (còn phiếu thì không hủy được kế hoạch).
    Task<int> DemPhieuConHieuLucAsync(int maKeHoach);
}
