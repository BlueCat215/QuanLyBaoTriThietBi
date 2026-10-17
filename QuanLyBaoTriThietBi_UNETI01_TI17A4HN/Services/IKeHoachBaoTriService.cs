// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Interface Service Kế hoạch bảo trì
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public interface IKeHoachBaoTriService
{
    Task<KeHoachBaoTriIndexViewModel> LayDanhSachAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    );

    /// Fail nếu không tìm thấy hoặc kế hoạch không còn sửa được (Đang thực hiện/Chờ nghiệm thu/Hoàn thành/Đã hủy).
    Task<ServiceResult<KeHoachBaoTriFormViewModel>> LayDeSuaAsync(int id);

    Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(int? giuLaiMa);

    /// Data = MaKeHoach vừa tạo.
    Task<ServiceResult<int>> ThemAsync(KeHoachBaoTriFormViewModel vm);
    Task<ServiceResult> SuaAsync(KeHoachBaoTriFormViewModel vm);

    /// Chuyển sang Đã hủy (giữ lịch sử, không xóa vật lý).
    Task<ServiceResult> HuyAsync(int id);

    /// Business rule chống trùng: không có 2 kế hoạch đang hoạt động cho cùng thiết bị & cùng kỳ (tháng-năm).
    /// Public để các chức năng khác (vòng đời trạng thái - việc 31) dùng lại.
    Task<ServiceResult> KiemTraTrungKeHoachAsync(
        int maThietBi,
        DateTime ngayDuKien,
        int? boQuaMaKeHoach = null
    );
}
