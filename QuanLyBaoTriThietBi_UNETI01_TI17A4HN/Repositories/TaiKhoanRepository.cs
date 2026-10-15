// Họ tên: <điền họ tên>
// MSSV: <điền MSSV>
// Nội dung: Module 1 - Repository Tài khoản (chỉ truy vấn dữ liệu bằng LINQ)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class TaiKhoanRepository : Repository<TaiKhoan>, ITaiKhoanRepository
{
    public TaiKhoanRepository(AppDbContext context) : base(context) { }

    public async Task<List<TaiKhoanRow>> TimKiemAsync(string? tuKhoa, VaiTroEnum? vaiTro, bool? trangThai)
    {
        var query = QueryNoTracking();

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var k = tuKhoa.Trim();
            query = query.Where(t => t.TenDangNhap.Contains(k) || t.HoTen.Contains(k) || t.Email.Contains(k));
        }

        if (vaiTro.HasValue)
            query = query.Where(t => t.VaiTro == vaiTro.Value);

        if (trangThai.HasValue)
            query = query.Where(t => t.TrangThai == trangThai.Value);

        return await query
            .OrderBy(t => t.VaiTro).ThenBy(t => t.HoTen)
            .Select(t => new TaiKhoanRow(
                t.MaTaiKhoan,
                t.TenDangNhap,
                t.HoTen,
                t.Email,
                t.VaiTro,
                t.KhuVuc != null ? t.KhuVuc.TenKhuVuc : null,
                t.TrangThai,
                t.KhoaDenNgay,
                t.LanDangNhapCuoi,
                t.BaoCaoSuCos.Any() || t.NhatKyDangNhaps.Any() || t.ThongBaos.Any()
                    || t.TepDinhKems.Any() || t.KyThuatVien != null))
            .ToListAsync();
    }

    public Task<bool> TenDangNhapDaTonTaiAsync(string tenDangNhap, int? boQuaId = null)
        => _dbSet.AnyAsync(t => t.TenDangNhap == tenDangNhap && (boQuaId == null || t.MaTaiKhoan != boQuaId));

    public Task<int> DemAdminHoatDongAsync(int? boQuaId = null)
        => _dbSet.CountAsync(t => t.VaiTro == VaiTroEnum.Admin && t.TrangThai
                                  && (boQuaId == null || t.MaTaiKhoan != boQuaId));

    public Task<bool> CoDuLieuLienQuanAsync(int maTaiKhoan)
        => _dbSet.AnyAsync(t => t.MaTaiKhoan == maTaiKhoan &&
            (t.BaoCaoSuCos.Any() || t.NhatKyDangNhaps.Any() || t.ThongBaos.Any()
             || t.TepDinhKems.Any() || t.KyThuatVien != null));

    public Task<bool> KhuVucDangHoatDongAsync(int maKhuVuc)
        => _context.KhuVucs.AnyAsync(k => k.MaKhuVuc == maKhuVuc && k.TrangThai);

    public async Task<List<KhuVucOption>> LayKhuVucChoChonAsync(int? giuLaiMa)
        => await _context.KhuVucs.AsNoTracking()
            .Where(k => k.TrangThai || k.MaKhuVuc == giuLaiMa)
            .OrderBy(k => k.TenKhuVuc)
            .Select(k => new KhuVucOption(k.MaKhuVuc, k.TenKhuVuc))
            .ToListAsync();
}
