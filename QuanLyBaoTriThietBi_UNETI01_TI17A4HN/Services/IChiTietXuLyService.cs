// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Service Chi tiết xử lý
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IChiTietXuLyService
{
    Task<ChiTietXuLyPhieuListViewModel> LayDanhSachPhieuAsync(TrangThaiPhieuXuLyEnum? trangThai);

    /// Chi tiết xử lý của 1 phiếu + tổng chi phí. Null nếu phiếu không tồn tại.
    Task<ChiTietXuLyIndexViewModel?> LayTheoPhieuAsync(int maPhieuXuLy);

    /// Form thêm mới cho 1 phiếu. Null nếu phiếu không tồn tại (Controller tự kiểm tra Phieu.ChoPhepGhiChiTiet).
    Task<ChiTietXuLyFormViewModel?> LayDeThemAsync(int maPhieuXuLy);

    /// Form sửa. Null nếu dòng chi tiết không tồn tại.
    Task<ChiTietXuLyFormViewModel?> LayDeSuaAsync(int maChiTiet);

    /// Nạp lại Phieu + danh sách vật tư khi hiển thị lại form có lỗi.
    Task NapDanhMucAsync(ChiTietXuLyFormViewModel vm);

    Task<ServiceResult> ThemAsync(ChiTietXuLyFormViewModel vm);
    Task<ServiceResult> SuaAsync(ChiTietXuLyFormViewModel vm);

    /// Xóa 1 dòng chi tiết (chỉ khi phiếu còn đang xử lý). Data = mã phiếu để Controller quay lại đúng trang.
    Task<ServiceResult<int>> XoaAsync(int maChiTiet);
}
