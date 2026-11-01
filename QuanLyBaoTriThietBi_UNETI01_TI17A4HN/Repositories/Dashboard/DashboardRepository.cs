// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Repository Dashboard (Truy vấn CSDL: ThietBi, KeHoach, BaoCao, PhieuXuLy, ChiPhi)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly AppDbContext _context;

    public DashboardRepository(AppDbContext context) => _context = context;

    public async Task<List<ThietBiDashboardDto>> LayDanhSachThietBiAsync()
    {
        return await _context.ThietBis
            .AsNoTracking()
            .Select(tb => new ThietBiDashboardDto(
                tb.MaThietBi,
                tb.TenThietBi,
                tb.SoSerial,
                tb.LoaiThietBi != null ? tb.LoaiThietBi.TenLoaiThietBi : null,
                tb.KhuVuc != null ? tb.KhuVuc.TenKhuVuc : null,
                tb.TinhTrang,
                tb.NgayBaoTriGanNhat,
                tb.NgayDuaVaoSuDung,
                tb.ChuKyBaoTriThang
            ))
            .ToListAsync();
    }

    public async Task<int> DemKeHoachChoThucHienAsync()
    {
        return await _context.KeHoachBaoTris
            .CountAsync(kh => kh.TrangThai == TrangThaiKeHoachEnum.ChoThucHien);
    }

    public async Task<Dictionary<TrangThaiBaoCaoEnum, int>> DemBaoCaoSuCoTheoTrangThaiAsync()
    {
        var groups = await _context.BaoCaoSuCos
            .AsNoTracking()
            .GroupBy(bc => bc.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        return groups.ToDictionary(g => g.TrangThai, g => g.SoLuong);
    }

    public async Task<List<BaoCaoChoXuLyItem>> LayBaoCaoChoXuLyAsync(int top = 5)
    {
        return await _context.BaoCaoSuCos
            .AsNoTracking()
            .Where(bc => bc.TrangThai == TrangThaiBaoCaoEnum.MoiBao || bc.TrangThai == TrangThaiBaoCaoEnum.DaTiepNhan)
            .OrderByDescending(bc => bc.NgayBaoCao)
            .Take(top)
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
    }

    public async Task<Dictionary<TrangThaiPhieuXuLyEnum, int>> DemPhieuXuLyTheoTrangThaiAsync()
    {
        var groups = await _context.PhieuXuLys
            .AsNoTracking()
            .GroupBy(p => p.TrangThai)
            .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
            .ToListAsync();

        return groups.ToDictionary(g => g.TrangThai, g => g.SoLuong);
    }

    public async Task<decimal> TinhTongChiPhiThangAsync(int thang, int nam)
    {
        return await _context.ChiTietXuLys
            .AsNoTracking()
            .Where(ct => ct.NgayCapNhat.Month == thang
                      && ct.NgayCapNhat.Year == nam
                      && ct.ThanhTien != null
                      && ct.PhieuXuLy != null
                      && ct.PhieuXuLy.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy)
            .SumAsync(ct => (decimal?)ct.ThanhTien) ?? 0m;
    }
}
