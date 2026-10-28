// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Service Chi tiết xử lý (CRUD chi tiết, tính tổng chi phí, bắt đầu xử lý chỉ KTV được phân công)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IChiTietXuLyService
{
    Task<ChiTietXuLyPhieuListViewModel> LayDanhSachPhieuAsync(TrangThaiPhieuXuLyEnum? trangThai);

    /// Chi tiết xử lý của 1 phiếu + tổng chi phí. Null nếu phiếu không tồn tại.
    Task<ChiTietXuLyIndexViewModel?> LayTheoPhieuAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap = null, bool laAdmin = false);

    /// Form thêm mới cho 1 phiếu. Null nếu phiếu không tồn tại.
    Task<ChiTietXuLyFormViewModel?> LayDeThemAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap = null, bool laAdmin = false);

    /// Form sửa. Null nếu dòng chi tiết không tồn tại.
    Task<ChiTietXuLyFormViewModel?> LayDeSuaAsync(int maChiTiet, int? maTaiKhoanDangNhap = null, bool laAdmin = false);

    /// Nạp lại Phieu + danh sách vật tư khi hiển thị lại form có lỗi.
    Task NapDanhMucAsync(ChiTietXuLyFormViewModel vm);

    /// Thêm dòng chi tiết (chỉ KTV đang được phân công hoặc Admin khi phiếu Đang xử lý).
    Task<ServiceResult> ThemAsync(ChiTietXuLyFormViewModel vm, int? maTaiKhoanDangNhap = null, bool laAdmin = false);

    /// Sửa dòng chi tiết (chỉ KTV đang được phân công hoặc Admin khi phiếu Đang xử lý).
    Task<ServiceResult> SuaAsync(ChiTietXuLyFormViewModel vm, int? maTaiKhoanDangNhap = null, bool laAdmin = false);

    /// Xóa 1 dòng chi tiết (chỉ khi phiếu còn đang xử lý). Data = mã phiếu để Controller quay lại đúng trang.
    Task<ServiceResult<int>> XoaAsync(int maChiTiet, int? maTaiKhoanDangNhap = null, bool laAdmin = false);

    /// <summary>
    /// STT 39: Bắt đầu xử lý (chỉ KTV đang được phân công hoặc Admin)
    /// Ghi NgayBatDau, chuyển phiếu sang Đang xử lý, cập nhật trạng thái nguồn và thiết bị.
    /// </summary>
    Task<ServiceResult> BatDauXuLyAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap, bool laAdmin);
}
