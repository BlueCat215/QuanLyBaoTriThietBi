// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Repository Báo cáo thống kê (Truy vấn CSDL cho các báo cáo đa chiều)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class ThongKeRepository : IThongKeRepository
{
    private readonly AppDbContext _context;

    public ThongKeRepository(AppDbContext context) => _context = context;

    public async Task<List<ThietBi>> LayDanhSachThietBiAsync()
    {
        return await _context.ThietBis
            .AsNoTracking()
            .Include(t => t.LoaiThietBi)
            .Include(t => t.KhuVuc)
            .ToListAsync();
    }

    public async Task<List<LoaiThietBi>> LayDanhSachLoaiThietBiAsync()
    {
        return await _context.LoaiThietBis.AsNoTracking().ToListAsync();
    }

    public async Task<List<KhuVuc>> LayDanhSachKhuVucAsync()
    {
        return await _context.KhuVucs.AsNoTracking().ToListAsync();
    }

    public async Task<List<KyThuatVien>> LayDanhSachKyThuatVienAsync()
    {
        return await _context.KyThuatViens.AsNoTracking().ToListAsync();
    }

    public async Task<List<BaoCaoSuCo>> LayBaoCaoSuCoTheoKhoangNgayAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.BaoCaoSuCos
            .AsNoTracking()
            .Where(b => b.NgayBaoCao >= startDate && b.NgayBaoCao < endDate)
            .ToListAsync();
    }

    public async Task<List<KeHoachBaoTri>> LayKeHoachBaoTriTheoKhoangNgayAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.KeHoachBaoTris
            .AsNoTracking()
            .Where(k => k.NgayDuKien >= startDate && k.NgayDuKien < endDate)
            .ToListAsync();
    }

    public async Task<List<PhieuXuLy>> LayPhieuXuLyTheoKhoangNgayAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.PhieuXuLys
            .AsNoTracking()
            .Include(p => p.BaoCaoSuCo)
            .Include(p => p.KeHoachBaoTri)
            .Include(p => p.PhanCongXuLys)
                .ThenInclude(pc => pc.KyThuatVien)
            .Include(p => p.ChiTietXuLys)
            .Where(p => p.NgayTao >= startDate && p.NgayTao < endDate)
            .ToListAsync();
    }

    public async Task<List<ChiTietXuLy>> LayChiTietXuLyTrongNamAsync(int nam)
    {
        return await _context.ChiTietXuLys
            .AsNoTracking()
            .Include(c => c.PhieuXuLy)
            .Where(c => c.NgayCapNhat.Year == nam
                     && c.ThanhTien != null
                     && c.PhieuXuLy != null
                     && c.PhieuXuLy.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy)
            .ToListAsync();
    }
}
