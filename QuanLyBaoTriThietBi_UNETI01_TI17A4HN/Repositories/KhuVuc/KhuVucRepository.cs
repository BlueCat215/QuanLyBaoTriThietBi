// Họ tên:  Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Repository Khu vực (chỉ truy vấn dữ liệu bằng LINQ)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class KhuVucRepository : Repository<KhuVuc>, IKhuVucRepository
{
    public KhuVucRepository(AppDbContext context) : base(context) { }

    public async Task<List<KhuVucRow>> TimKiemAsync(string? tuKhoa, bool? trangThai)
    {
        var query = QueryNoTracking();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(v => v.TenKhuVuc.Contains(k)
                                     || (v.ViTri != null && v.ViTri.Contains(k))
                                     || (v.NguoiPhuTrach != null && v.NguoiPhuTrach.Contains(k)));
        }

        if (trangThai.HasValue)
            query = query.Where(v => v.TrangThai == trangThai.Value);

        return await query
            .OrderBy(v => v.TenKhuVuc)
            .Select(v => new KhuVucRow(
                v.MaKhuVuc,
                v.TenKhuVuc,
                v.ViTri,
                v.NguoiPhuTrach,
                v.TrangThai,
                v.ThietBis.Count,
                v.TaiKhoans.Count))
            .ToListAsync();
    }

    public Task<bool> TenDaTonTaiAsync(string ten, int? boQuaId = null)
        => _dbSet.AnyAsync(v => v.TenKhuVuc == ten && (boQuaId == null || v.MaKhuVuc != boQuaId));

    public Task<int> DemThietBiAsync(int maKhuVuc)
        => _context.ThietBis.CountAsync(t => t.MaKhuVuc == maKhuVuc);

    public Task<int> DemTaiKhoanAsync(int maKhuVuc)
        => _context.TaiKhoans.CountAsync(t => t.MaKhuVuc == maKhuVuc);
}
