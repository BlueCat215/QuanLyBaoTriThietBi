// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Interface Service Kế hoạch bảo trì (CRUD, Vòng đời trạng thái, Chuyển trạng thái, Chống trùng kế hoạch, Phát sinh Phiếu xử lý)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IKeHoachBaoTriService
{
    /// Lấy danh sách kế hoạch đầy đủ tiêu chí: tìm kiếm, lọc trạng thái/ưu tiên/khoảng ngày, sắp xếp, phân trang
    Task<KeHoachBaoTriIndexViewModel> LayDanhSachAsync(KeHoachBaoTriBoLoc boLoc);

    Task<KeHoachBaoTriIndexViewModel> LayDanhSachAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    );

    /// Lấy chi tiết kế hoạch theo id kèm thông tin thiết bị và danh sách phiếu xử lý liên kết
    Task<ServiceResult<KeHoachBaoTriDetailsViewModel>> LayChiTietAsync(int id);

    /// Fail nếu không tìm thấy hoặc kế hoạch không còn sửa được (Đang thực hiện/Chờ nghiệm thu/Hoàn thành/Đã hủy).
    Task<ServiceResult<KeHoachBaoTriFormViewModel>> LayDeSuaAsync(int id);

    Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(int? giuLaiMa);

    /// Data = MaKeHoach vừa tạo.
    Task<ServiceResult<int>> ThemAsync(KeHoachBaoTriFormViewModel vm);
    Task<ServiceResult> SuaAsync(KeHoachBaoTriFormViewModel vm);

    /// Chuyển sang Đã hủy (giữ lịch sử, không xóa vật lý).
    Task<ServiceResult> HuyAsync(int id);

    /// Chuyển đổi trạng thái theo đúng vòng đời nghiệp vụ:
    /// Chờ thực hiện -> Đã phân công -> Đang thực hiện -> Chờ nghiệm thu -> Hoàn thành / Đã hủy.
    /// Chặn cập nhật khi đã Hoàn thành hoặc Đã hủy.
    Task<ServiceResult> ChuyenTrangThaiAsync(
        int id,
        TrangThaiKeHoachEnum trangThaiMoi,
        string? ghiChu = null
    );

    /// Phát sinh Phiếu xử lý dạng bảo trì định kỳ từ Kế hoạch bảo trì
    Task<ServiceResult<int>> TaoPhieuXuLyDinhKyAsync(int maKeHoach);

    /// Nghiệm thu / Hoàn thành kế hoạch:
    /// - Chuyển trạng thái kế hoạch = Hoàn thành
    /// - Cập nhật NgayBaoTriGanNhat của Thiết bị = ngày hoàn thành thực tế
    /// - NgayBaoTriTiepTheo tự động tính lại từ mốc mới qua công thức động
    /// - Bảo toàn lịch sử các lần bảo trì trước
    Task<ServiceResult> HoanThanhAsync(
        int maKeHoach,
        DateTime ngayHoanThanh,
        string? ghiChu = null
    );

    /// Business rule chống trùng: không có 2 kế hoạch đang hoạt động cho cùng thiết bị & cùng kỳ (tháng-năm).
    Task<ServiceResult> KiemTraTrungKeHoachAsync(
        int maThietBi,
        DateTime ngayDuKien,
        int? boQuaMaKeHoach = null
    );
}
