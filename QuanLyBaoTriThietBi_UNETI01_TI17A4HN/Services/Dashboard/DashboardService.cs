// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Service Dashboard (SST 44: số liệu thật từ DB - 8 chỉ số tổng quan + danh sách hành động)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

/// SST 44: Thay thân LayThongKeAsync bằng truy vấn LINQ thật.
/// NgayBaoTriTiepTheo là [NotMapped] nên load thiết bị về memory rồi tính bằng UiHelper.PhanLoaiHanBaoTri.
/// LaDuLieuMinhHoa = false.
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context) => _context = context;

    public async Task<DashboardViewModel> LayThongKeAsync(int? thang = null, int? nam = null)
    {
        var homNay = DateTime.Today;
        int targetThang = (thang is >= 1 and <= 12) ? thang.Value : homNay.Month;
        int targetNam = (nam is >= 2000 and <= 2100) ? nam.Value : homNay.Year;

        // ---- 1 + 2: Tổng thiết bị và phân loại theo tình trạng ----
        var thietBiList = await _context.ThietBis
            .AsNoTracking()
            .Select(tb => new
            {
                tb.MaThietBi,
                tb.TenThietBi,
                tb.SoSerial,
                TenLoaiThietBi = tb.LoaiThietBi != null ? tb.LoaiThietBi.TenLoaiThietBi : null,
                TenKhuVuc = tb.KhuVuc != null ? tb.KhuVuc.TenKhuVuc : null,
                tb.TinhTrang,
                tb.NgayBaoTriGanNhat,
                tb.NgayDuaVaoSuDung,
                tb.ChuKyBaoTriThang,
            })
            .ToListAsync();

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
        int keHoachChoThucHien = await _context.KeHoachBaoTris
            .CountAsync(kh => kh.TrangThai == TrangThaiKeHoachEnum.ChoThucHien);

        // ---- 6: Báo cáo sự cố theo trạng thái ----
        var suCoGroups = await _context.BaoCaoSuCos
            .AsNoTracking()
            .GroupBy(bc => bc.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        var suCoDict = Enum.GetValues<TrangThaiBaoCaoEnum>()
            .ToDictionary(v => v, v => suCoGroups.FirstOrDefault(g => g.TrangThai == v)?.SoLuong ?? 0);

        // Danh sách sự cố mới / đã tiếp nhận đang chờ xử lý
        var suCoMoiCanXuLy = await _context.BaoCaoSuCos
            .AsNoTracking()
            .Where(bc => bc.TrangThai == TrangThaiBaoCaoEnum.MoiBao || bc.TrangThai == TrangThaiBaoCaoEnum.DaTiepNhan)
            .OrderByDescending(bc => bc.NgayBaoCao)
            .Take(5)
            .Select(bc => new BaoCaoChoXuLyItem(
                bc.MaBaoCao,
                bc.MaThietBi,
                bc.ThietBi != null ? bc.ThietBi.TenThietBi : "—",
                bc.ThietBi != null ? bc.ThietBi.SoSerial : "—",
                bc.MoTaSuCo,
                bc.MucDoSuCo,
                bc.TrangThai,
                bc.NgayBaoCao,
                bc.TaiKhoanBaoCao != null ? bc.TaiKhoanBaoCao.HoTen : "—"
            ))
            .ToListAsync();

        // ---- 7: Phiếu xử lý theo trạng thái ----
        var phieuGroups = await _context.PhieuXuLys
            .AsNoTracking()
            .GroupBy(p => p.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        var phieuDict = Enum.GetValues<TrangThaiPhieuXuLyEnum>()
            .ToDictionary(v => v, v => phieuGroups.FirstOrDefault(g => g.TrangThai == v)?.SoLuong ?? 0);

        // ---- 8: Tổng chi phí tháng được chọn (bỏ qua các phiếu đã hủy) ----
        decimal tongChiPhiThang = await _context.ChiTietXuLys
            .AsNoTracking()
            .Where(ct => ct.NgayCapNhat.Month == targetThang
                      && ct.NgayCapNhat.Year == targetNam
                      && ct.ThanhTien != null
                      && ct.PhieuXuLy != null
                      && ct.PhieuXuLy.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy)
            .SumAsync(ct => (decimal?)ct.ThanhTien) ?? 0m;

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

    /// Chuyển Dictionary enum -> danh sách mục theo ĐÚNG thứ tự khai báo enum; enum thiếu số liệu = 0.
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
