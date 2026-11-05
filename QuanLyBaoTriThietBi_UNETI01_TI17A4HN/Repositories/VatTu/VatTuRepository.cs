// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - STT 57: Triển khai Repository Quản lý kho vật tư
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class VatTuRepository : Repository<VatTu>, IVatTuRepository
{
    public VatTuRepository(AppDbContext context) : base(context) { }

    private IQueryable<VatTu> TaoTruyVanLoc(VatTuFilterViewModel filter)
    {
        var query = _context.VatTus.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(filter.TuKhoa))
        {
            var keyword = filter.TuKhoa.Trim();
            query = query.Where(v => v.TenVatTu.Contains(keyword) || v.DonViTinh.Contains(keyword));
        }

        if (filter.TrangThai.HasValue)
        {
            query = query.Where(v => v.TrangThai == filter.TrangThai.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.MucTon))
        {
            query = filter.MucTon switch
            {
                "HetHang" => query.Where(v => v.SoLuongTon <= 0),
                "SapHet" => query.Where(v => v.SoLuongTon > 0 && v.SoLuongTon <= 5),
                "ConHang" => query.Where(v => v.SoLuongTon > 0),
                _ => query
            };
        }

        return query;
    }

    public async Task<(List<VatTu> Items, int TotalCount)> LayDanhSachAsync(VatTuFilterViewModel filter)
    {
        var query = TaoTruyVanLoc(filter);

        var total = await query.CountAsync();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

        var items = await query
            .Include(v => v.ChiTietXuLys)
            .OrderBy(v => v.SoLuongTon) // Ưu tiên các vật tư ít tồn lên trước để dễ quan sát
            .ThenBy(v => v.TenVatTu)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public Task<bool> TenDaTonTaiAsync(string tenVatTu, int? loaiTruMaVatTu = null)
    {
        var tenChuan = tenVatTu.Trim();
        return _context.VatTus.AnyAsync(v => 
            v.TenVatTu == tenChuan && (!loaiTruMaVatTu.HasValue || v.MaVatTu != loaiTruMaVatTu.Value)
        );
    }

    public Task<int> DemSoLanSuDungAsync(int maVatTu)
    {
        return _context.ChiTietXuLys.CountAsync(c => c.MaVatTu == maVatTu);
    }

    public async Task<(int TongSo, int SapHet, int HetHang, decimal TongGiaTri)> LayThongKeKhoAsync()
    {
        var list = await _context.VatTus.AsNoTracking().ToListAsync();
        var tongSo = list.Count;
        var sapHet = list.Count(v => v.SoLuongTon > 0 && v.SoLuongTon <= 5);
        var hetHang = list.Count(v => v.SoLuongTon <= 0);
        var tongGiaTri = list.Sum(v => v.SoLuongTon * v.DonGiaMacDinh);

        return (tongSo, sapHet, hetHang, tongGiaTri);
    }

    public async Task<List<VatTu>> LayTatCaDeXuatExcelAsync(VatTuFilterViewModel filter)
    {
        var query = TaoTruyVanLoc(filter);
        return await query
            .OrderBy(v => v.TenVatTu)
            .ToListAsync();
    }

    public Task<VatTu?> LayTheoMaTrackingAsync(int maVatTu)
    {
        return _context.VatTus.FirstOrDefaultAsync(v => v.MaVatTu == maVatTu);
    }
}
