// Repository Thiết bị (LINQ: tìm kiếm, kiểm tra trùng serial, đếm dữ liệu liên quan)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class ThietBiRepository : Repository<ThietBi>, IThietBiRepository
{
    public ThietBiRepository(AppDbContext context)
        : base(context) { }

    // Tìm kiếm, Lọc, Sắp xếp (xem ThietBiQueryExtensions)
    public IQueryable<ThietBi> TruyVan(ThietBiBoLoc boLoc)
    {
        IQueryable<ThietBi> query = QueryNoTracking()
            .Include(t => t.LoaiThietBi)
            .Include(t => t.KhuVuc);

        // Thứ tự bắt buộc: Where -> OrderBy (-> Skip -> Take ở việc 28)
        return query.ApDungLoc(boLoc, DateTime.Today).ApDungSapXep(boLoc.SapXep);
    }

    public Task<List<ThietBi>> TimKiemAsync(ThietBiBoLoc boLoc) => TruyVan(boLoc).ToListAsync();

    public Task<ThietBi?> LayChiTietAsync(int maThietBi) =>
        QueryNoTracking()
            .Include(t => t.LoaiThietBi)
            .Include(t => t.KhuVuc)
            .FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);

    public Task<bool> SoSerialDaTonTaiAsync(string soSerial, int? boQuaId = null) =>
        _dbSet.AnyAsync(t => t.SoSerial == soSerial && (boQuaId == null || t.MaThietBi != boQuaId));

    public async Task<DuLieuLienQuanThietBi> DemDuLieuLienQuanAsync(int maThietBi)
    {
        var soKeHoach = await _context.KeHoachBaoTris.CountAsync(k => k.MaThietBi == maThietBi);
        var soBaoCao = await _context.BaoCaoSuCos.CountAsync(b => b.MaThietBi == maThietBi);
        var soThongBao = await _context.ThongBaos.CountAsync(n => n.MaThietBi == maThietBi);
        var soTep = await _context.TepDinhKems.CountAsync(f =>
            f.LoaiDoiTuong == LoaiDoiTuongDinhKemEnum.ThietBi && f.MaDoiTuong == maThietBi
        );

        return new DuLieuLienQuanThietBi(soKeHoach, soBaoCao, soThongBao, soTep);
    }

    public Task<bool> LoaiDangHoatDongAsync(int maLoaiThietBi) =>
        _context.LoaiThietBis.AnyAsync(l => l.MaLoaiThietBi == maLoaiThietBi && l.TrangThai);

    public Task<bool> KhuVucDangHoatDongAsync(int maKhuVuc) =>
        _context.KhuVucs.AnyAsync(k => k.MaKhuVuc == maKhuVuc && k.TrangThai);

    public async Task<List<(int Ma, string Ten)>> LayLoaiDeLocAsync()
    {
        var ds = await _context
            .LoaiThietBis.AsNoTracking()
            .OrderBy(l => l.TenLoaiThietBi)
            .Select(l => new { l.MaLoaiThietBi, l.TenLoaiThietBi })
            .ToListAsync();
        return ds.Select(x => (x.MaLoaiThietBi, x.TenLoaiThietBi)).ToList();
    }

    public async Task<List<(int Ma, string Ten)>> LayKhuVucDeLocAsync()
    {
        var ds = await _context
            .KhuVucs.AsNoTracking()
            .OrderBy(k => k.TenKhuVuc)
            .Select(k => new { k.MaKhuVuc, k.TenKhuVuc })
            .ToListAsync();
        return ds.Select(x => (x.MaKhuVuc, x.TenKhuVuc)).ToList();
    }

    public async Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa)
    {
        var ds = await _context
            .LoaiThietBis.AsNoTracking()
            .Where(l => l.TrangThai || l.MaLoaiThietBi == giuLaiMa)
            .OrderBy(l => l.TenLoaiThietBi)
            .Select(l => new { l.MaLoaiThietBi, l.TenLoaiThietBi })
            .ToListAsync();
        return ds.Select(x => (x.MaLoaiThietBi, x.TenLoaiThietBi)).ToList();
    }

    public async Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa)
    {
        var ds = await _context
            .KhuVucs.AsNoTracking()
            .Where(k => k.TrangThai || k.MaKhuVuc == giuLaiMa)
            .OrderBy(k => k.TenKhuVuc)
            .Select(k => new { k.MaKhuVuc, k.TenKhuVuc })
            .ToListAsync();
        return ds.Select(x => (x.MaKhuVuc, x.TenKhuVuc)).ToList();
    }
}
