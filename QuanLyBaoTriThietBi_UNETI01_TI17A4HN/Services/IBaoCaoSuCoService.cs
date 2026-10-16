// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Interface Service Báo cáo sự cố
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IBaoCaoSuCoService
{
    /// maTaiKhoanLoc = null: toàn bộ (Admin); có giá trị: chỉ báo cáo của tài khoản đó ("Báo cáo của tôi").
    Task<BaoCaoSuCoIndexViewModel> LayDanhSachAsync(
        string? tuKhoa,
        TrangThaiBaoCaoEnum? trangThai,
        MucDoSuCoEnum? mucDo,
        int? maTaiKhoanLoc
    );

    /// null nếu không tồn tại HOẶC người dùng không có quyền xem (không phải Admin và không phải chủ báo cáo).
    Task<BaoCaoSuCoDetailsViewModel?> LayChiTietAsync(int id, NguoiThaoTac nguoi);

    /// Fail nếu không tìm thấy/không có quyền/không còn ở trạng thái Mới báo.
    Task<ServiceResult<BaoCaoSuCoFormViewModel>> LayDeSuaAsync(int id, NguoiThaoTac nguoi);

    Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(NguoiThaoTac nguoi, int? giuLaiMa);

    /// Data = MaBaoCao vừa tạo.
    Task<ServiceResult<int>> GuiBaoCaoAsync(BaoCaoSuCoFormViewModel vm, NguoiThaoTac nguoi);
    Task<ServiceResult> SuaAsync(BaoCaoSuCoFormViewModel vm, NguoiThaoTac nguoi);

    /// Chuyển sang Đã hủy (giữ lịch sử).
    Task<ServiceResult> HuyAsync(int id, NguoiThaoTac nguoi);

    /// Chỉ xóa được báo cáo mới báo, chưa phát sinh phiếu xử lý/tệp đính kèm.
    Task<ServiceResult> XoaAsync(int id, NguoiThaoTac nguoi);

    /// Chỉ để xem thử khi chưa có đăng nhập (xem Controller).
    Task<int?> LayMaTaiKhoanXemThuAsync();
}
