// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Interface Repository Báo cáo sự cố
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IBaoCaoSuCoRepository : IRepository<BaoCaoSuCo>
{
    /// Tìm theo thiết bị (tên/serial) / mô tả / người báo + lọc trạng thái, mức độ. maTaiKhoan != null = chỉ báo cáo của người đó.
    Task<List<BaoCaoSuCoRow>> TimKiemAsync(
        string? tuKhoa,
        TrangThaiBaoCaoEnum? trangThai,
        MucDoSuCoEnum? mucDo,
        int? maTaiKhoan
    );

    /// Báo cáo kèm Thiết bị (Khu vực, Loại) và Người báo; không tracking.
    Task<BaoCaoSuCo?> LayChiTietAsync(int maBaoCao);

    Task<List<PhieuXuLyTomTat>> LayPhieuXuLyAsync(int maBaoCao);
    Task<DuLieuLienQuanBaoCao> DemDuLieuLienQuanAsync(int maBaoCao);

    Task<ThietBi?> LayThietBiAsync(int maThietBi);
    Task<TaiKhoan?> LayTaiKhoanAsync(int maTaiKhoan);

    /// Tài khoản người dùng đầu tiên còn hoạt động - chỉ dùng cho chế độ xem thử khi chưa có đăng nhập.
    Task<int?> LayMaTaiKhoanNguoiDungDauTienAsync();

    /// Thiết bị chưa Ngừng sử dụng; gioiHanKhuVuc = true thì chỉ lấy thiết bị thuộc maKhuVuc. Luôn giữ lại giuLaiMa.
    Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(
        bool gioiHanKhuVuc,
        int? maKhuVuc,
        int? giuLaiMa
    );
}
