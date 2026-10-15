// Họ tên:  Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Repository Loại thiết bị (chỉ truy vấn dữ liệu bằng LINQ)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class LoaiThietBiRepository : Repository<LoaiThietBi>, ILoaiThietBiRepository
{
    public LoaiThietBiRepository(AppDbContext context) : base(context) { }

    public async Task<List<LoaiThietBiRow>> TimKiemAsync(string? tuKhoa, bool? trangThai)
    {
        var query = QueryNoTracking();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(l => l.TenLoaiThietBi.Contains(k) || (l.MoTa != null && l.MoTa.Contains(k)));
        }

        if (trangThai.HasValue)
            query = query.Where(l => l.TrangThai == trangThai.Value);

        return await query
            .OrderBy(l => l.TenLoaiThietBi)
            .Select(l => new LoaiThietBiRow(
                l.MaLoaiThietBi,
                l.TenLoaiThietBi,
                l.ChuKyBaoTriMacDinhThang,
                l.MoTa,
                l.TrangThai,
                l.ThietBis.Count))
            .ToListAsync();
    }

    public Task<bool> TenDaTonTaiAsync(string ten, int? boQuaId = null)
        => _dbSet.AnyAsync(l => l.TenLoaiThietBi == ten && (boQuaId == null || l.MaLoaiThietBi != boQuaId));

    public Task<int> DemThietBiAsync(int maLoaiThietBi)
        => _context.ThietBis.CountAsync(t => t.MaLoaiThietBi == maLoaiThietBi);
}
