// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Service Dashboard (SST 44: số liệu thật từ Repository - 8 chỉ số tổng quan + danh sách hành động)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

/// <summary>
/// SST 44: Xử lý logic Dashboard dựa trên số liệu từ IDashboardRepository.
/// NgayBaoTriTiepTheo là [NotMapped] nên tính bằng UiHelper.PhanLoaiHanBaoTri.
/// LaDuLieuMinhHoa = false.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;

    public DashboardService(IDashboardRepository repo) => _repo = repo;

    public async Task<DashboardViewModel> LayThongKeAsync(int? thang = null, int? nam = null)
    {
        var homNay = DateTime.Today;
        int targetThang = (thang is >= 1 and <= 12) ? thang.Value : homNay.Month;
        int targetNam = (nam is >= 2000 and <= 2100) ? nam.Value : homNay.Year;

        // ---- 1 + 2: Tổng thiết bị và phân loại theo tình trạng ----
        var thietBiList = await _repo.LayDanhSachThietBiAsync();

        var tinhTrangDict = Enum.GetValues<TinhTrangThietBiEnum>()
            .ToDictionary(v => v, v => thietBiList.Count(tb => tb.TinhTrang == v));

        // ---- 3 + 4: Sắp đến hạn / Quá hạn bảo trì ----
        // Thiết bị Ngừng sử dụng không tính hạn bảo trì (theo TrangThaiHanBaoTri? của entity ThietBi)
        var thietBiCanTinhHan = thietBiList
            .Where(tb => tb.TinhTrang != TinhTrangThietBiEnum.NgungSuDung)
            .Select(tb =>
            {
                var ngayTiepTheo = (tb.NgayBaoTriGanNhat ?? tb.NgayDuaVaoSuDung).AddMonths(tb.ChuKyBaoTriThang);
                var trangThaiHan = UiHelper.PhanLoaiHanBaoTri(ngayTiepTheo, homNay);
                var soNgayLech = (ngayTiepTheo.Date - homNay).Days;
                return new
                {
                    tb.MaThietBi,
                    tb.TenThietBi,
                    tb.SoSerial,
                    tb.TenLoaiThietBi,
                    tb.TenKhuVuc,
                    NgayTiepTheo = ngayTiepTheo,
                    TrangThaiHan = trangThaiHan,
                    SoNgayLech = soNgayLech,
                };
            })
            .ToList();

        int thietBiSapDenHan = thietBiCanTinhHan.Count(t => t.TrangThaiHan == TrangThaiHanBaoTriEnum.SapDenHan);
        int thietBiQuaHan = thietBiCanTinhHan.Count(t => t.TrangThaiHan == TrangThaiHanBaoTriEnum.QuaHan);

        // Danh sách thiết bị cần bảo trì gấp: Ưu tiên Quá hạn (lệch nhiều nhất lên đầu), tiếp đến Sắp đến hạn
        var thietBiCanBaoTri = thietBiCanTinhHan
            .Where(t => t.TrangThaiHan is TrangThaiHanBaoTriEnum.QuaHan or TrangThaiHanBaoTriEnum.SapDenHan)
            .OrderBy(t => t.TrangThaiHan == TrangThaiHanBaoTriEnum.QuaHan ? 0 : 1)
            .ThenBy(t => t.NgayTiepTheo)
            .Take(5)
            .Select(t => new ThietBiCanBaoTriItem(
                t.MaThietBi,
                t.TenThietBi,
                t.SoSerial,
                t.TenLoaiThietBi,
                t.TenKhuVuc,
                t.NgayTiepTheo,
                t.TrangThaiHan,
                t.SoNgayLech
            ))
            .ToList();

        // ---- 5: Kế hoạch bảo trì chờ thực hiện ----
        int keHoachChoThucHien = await _repo.DemKeHoachChoThucHienAsync();

        // ---- 6: Báo cáo sự cố theo trạng thái ----
        var suCoDict = await _repo.DemBaoCaoSuCoTheoTrangThaiAsync();

        // Danh sách sự cố mới / đã tiếp nhận đang chờ xử lý
        var suCoMoiCanXuLy = await _repo.LayBaoCaoChoXuLyAsync(5);

        // ---- 7: Phiếu xử lý theo trạng thái ----
        var phieuDict = await _repo.DemPhieuXuLyTheoTrangThaiAsync();

        // ---- 8: Tổng chi phí tháng được chọn (bỏ qua các phiếu đã hủy) ----
        decimal tongChiPhiThang = await _repo.TinhTongChiPhiThangAsync(targetThang, targetNam);

        return new DashboardViewModel
        {
            LaDuLieuMinhHoa = false,

            // 1+2
            TongThietBi = thietBiList.Count,
            ThietBiTheoTinhTrang = TheoEnum<TinhTrangThietBiEnum>(
                tinhTrangDict,
                UiHelper.Badge,
                v => $"/ThietBi?TinhTrang={v}"
            ),

            // 3+4
            ThietBiSapDenHan = thietBiSapDenHan,
            ThietBiQuaHan = thietBiQuaHan,

            // 5
            KeHoachChoThucHien = keHoachChoThucHien,

            // 6
            SuCoTheoTrangThai = TheoEnum<TrangThaiBaoCaoEnum>(
                suCoDict,
                UiHelper.Badge,
                v => $"/BaoCaoSuCo?trangThai={v}"
            ),

            // 7
            PhieuTheoTrangThai = TheoEnum<TrangThaiPhieuXuLyEnum>(
                phieuDict,
                UiHelper.Badge,
                v => $"/PhieuXuLy?TrangThai={v}"
            ),

            // 8
            TongChiPhiThang = tongChiPhiThang,
            Thang = targetThang,
            Nam = targetNam,

            // Danh sách hành động
            ThietBiCanBaoTri = thietBiCanBaoTri,
            SuCoMoiCanXuLy = suCoMoiCanXuLy,
        };
    }

    /// <summary>
    /// Chuyển Dictionary enum -> danh sách mục theo ĐÚNG thứ tự khai báo enum; enum thiếu số liệu = 0.
    /// </summary>
    private static List<ThongKeMuc> TheoEnum<TEnum>(
        IReadOnlyDictionary<TEnum, int> soLieu,
        Func<TEnum, BadgeInfo> badge,
        Func<TEnum, string>? linkGenerator = null
    )
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(v =>
            {
                var b = badge(v);
                return new ThongKeMuc(
                    b.Text,
                    soLieu.GetValueOrDefault(v),
                    UiHelper.MauCot(b),
                    linkGenerator?.Invoke(v)
                );
            })
            .ToList();
}
