// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu
// MSSV: 23103100207 - 23103100225
// Nội dung: Module 3 & 5 - STT 59: Repository Thông báo nội bộ (In-App Notification)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class ThongBaoRepository : Repository<ThongBao>, IThongBaoRepository
{
    public ThongBaoRepository(AppDbContext context) : base(context) { }

    public Task<int> DemSoChuaDocAsync(int maTaiKhoan) =>
        _context.ThongBaos
            .AsNoTracking()
            .CountAsync(t => t.MaTaiKhoanNhan == maTaiKhoan 
                          && t.Kenh == KenhThongBaoEnum.HeThong 
                          && !t.DaDoc);

    public async Task<List<ThongBaoItemDto>> LayDanhSachGanNhatAsync(int maTaiKhoan, int soLuong = 10)
    {
        var query = _context.ThongBaos
            .AsNoTracking()
            .Where(t => t.MaTaiKhoanNhan == maTaiKhoan && t.Kenh == KenhThongBaoEnum.HeThong)
            .OrderByDescending(t => t.NgayTao)
            .Take(soLuong)
            .Select(t => new ThongBaoItemDto
            {
                MaThongBao = t.MaThongBao,
                NoiDung = t.NoiDung,
                NgayTao = t.NgayTao,
                DaDoc = t.DaDoc,
                MaThietBi = t.MaThietBi,
                TenThietBi = t.ThietBi != null ? t.ThietBi.TenThietBi : null,
                SoSerial = t.ThietBi != null ? t.ThietBi.SoSerial : null
            });

        return await query.ToListAsync();
    }

    public async Task<PagedResult<ThongBaoItemDto>> LayPhanTrangAsync(
        int maTaiKhoan, 
        bool? chiChuaDoc, 
        int trang = 1, 
        int soBanGhi = 15)
    {
        var query = _context.ThongBaos
            .AsNoTracking()
            .Where(t => t.MaTaiKhoanNhan == maTaiKhoan && t.Kenh == KenhThongBaoEnum.HeThong);

        if (chiChuaDoc == true)
        {
            query = query.Where(t => !t.DaDoc);
        }

        var total = await query.CountAsync();
        var page = trang < 1 ? 1 : trang;
        var pageSize = soBanGhi < 1 ? 15 : soBanGhi;

        var items = await query
            .OrderByDescending(t => t.NgayTao)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new ThongBaoItemDto
            {
                MaThongBao = t.MaThongBao,
                NoiDung = t.NoiDung,
                NgayTao = t.NgayTao,
                DaDoc = t.DaDoc,
                MaThietBi = t.MaThietBi,
                TenThietBi = t.ThietBi != null ? t.ThietBi.TenThietBi : null,
                SoSerial = t.ThietBi != null ? t.ThietBi.SoSerial : null
            })
            .ToListAsync();

        return new PagedResult<ThongBaoItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total
        };
    }

    public async Task<bool> DanhDauDaDocAsync(int maThongBao, int maTaiKhoan)
    {
        var tb = await _context.ThongBaos
            .FirstOrDefaultAsync(t => t.MaThongBao == maThongBao && t.MaTaiKhoanNhan == maTaiKhoan);

        if (tb == null) return false;
        if (tb.DaDoc) return true;

        tb.DaDoc = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> DanhDauTatCaDaDocAsync(int maTaiKhoan)
    {
        var danhSach = await _context.ThongBaos
            .Where(t => t.MaTaiKhoanNhan == maTaiKhoan && t.Kenh == KenhThongBaoEnum.HeThong && !t.DaDoc)
            .ToListAsync();

        if (danhSach.Count == 0) return 0;

        foreach (var tb in danhSach)
        {
            tb.DaDoc = true;
        }

        await _context.SaveChangesAsync();
        return danhSach.Count;
    }

    public async Task ThemThongBaoAsync(ThongBao thongBao)
    {
        await _context.ThongBaos.AddAsync(thongBao);
        await _context.SaveChangesAsync();
    }

    public async Task ThemNhieuThongBaoAsync(IEnumerable<ThongBao> danhSach)
    {
        await _context.ThongBaos.AddRangeAsync(danhSach);
        await _context.SaveChangesAsync();
    }
}
