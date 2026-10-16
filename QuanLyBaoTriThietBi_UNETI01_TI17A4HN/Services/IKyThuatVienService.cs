// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Interface Service Kỹ thuật viên
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IKyThuatVienService
{
    Task<KyThuatVienIndexViewModel> LayDanhSachAsync(string? tuKhoa, bool? trangThai);
    Task<KyThuatVienFormViewModel?> LayDeSuaAsync(int id);
    Task<List<(int Ma, string Ten)>> LayTaiKhoanChoChonAsync(int? giuLaiMa);

    Task<ServiceResult> ThemAsync(KyThuatVienFormViewModel vm);
    Task<ServiceResult> SuaAsync(KyThuatVienFormViewModel vm);

    /// Đảo trạng thái Hoạt động <-> Ngừng hoạt động.
    Task<ServiceResult<KetQuaDoiTrangThaiKyThuatVien>> DoiTrangThaiAsync(int id);

    /// Chỉ xóa được KTV chưa từng được phân công; còn lại phải chuyển Ngừng hoạt động.
    Task<ServiceResult> XoaAsync(int id);

    // ---- Dùng cho Phân công xử lý (việc 35, tuần 3) ----

    /// KTV phải tồn tại và đang hoạt động mới được nhận phân công mới.
    Task<ServiceResult> KiemTraNhanPhanCongAsync(int maKyThuatVien);

    /// Danh sách KTV đang hoạt động để chọn khi phân công / đổi KTV.
    Task<List<(int Ma, string Ten)>> LayDangHoatDongChoChonAsync();
}
