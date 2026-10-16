// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Repository Kỹ thuật viên (LINQ: tìm kiếm, kiểm tra trùng, đếm phân công)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class KyThuatVienRepository : Repository<KyThuatVien>, IKyThuatVienRepository
{
    private const int SoTuKhoaToiDa = 5;

    public KyThuatVienRepository(AppDbContext context)
        : base(context) { }

    public async Task<List<KyThuatVienRow>> TimKiemAsync(string? tuKhoa, bool? trangThai)
    {
        IQueryable<KyThuatVien> query = QueryNoTracking();

        if (trangThai is { } tt)
            query = query.Where(k => k.TrangThai == tt);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            // Mỗi từ phải khớp ít nhất 1 cột (AND giữa các từ, OR giữa các cột)
            var cacTu = tuKhoa
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(SoTuKhoaToiDa);

            foreach (var tu in cacTu)
            {
                query = query.Where(k =>
                    k.HoTen.Contains(tu)
                    || (k.ChuyenMon != null && k.ChuyenMon.Contains(tu))
                    || (k.SoDienThoai != null && k.SoDienThoai.Contains(tu))
                    || (k.Email != null && k.Email.Contains(tu))
                    || (k.TaiKhoan != null && k.TaiKhoan.TenDangNhap.Contains(tu))
                );
            }
        }

        var ds = await query
            .OrderByDescending(k => k.TrangThai)
            .ThenBy(k => k.HoTen)
            .ThenBy(k => k.MaKyThuatVien)
            .Select(k => new
            {
                k.MaKyThuatVien,
                k.HoTen,
                k.ChuyenMon,
                k.SoDienThoai,
                k.Email,
                k.TrangThai,
                TenDangNhap = k.TaiKhoan != null ? k.TaiKhoan.TenDangNhap : null,
                DangHieuLuc = k.PhanCongXuLys.Count(p => p.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc),
                Tong = k.PhanCongXuLys.Count(),
            })
            .ToListAsync();

        return ds.Select(x => new KyThuatVienRow(
                x.MaKyThuatVien,
                x.HoTen,
                x.ChuyenMon,
                x.SoDienThoai,
                x.Email,
                x.TrangThai,
                x.TenDangNhap,
                x.DangHieuLuc,
                x.Tong
            ))
            .ToList();
    }

    public Task<bool> EmailDaTonTaiAsync(string email, int? boQuaId = null) =>
        _dbSet.AnyAsync(k => k.Email == email && (boQuaId == null || k.MaKyThuatVien != boQuaId));

    public Task<int> DemPhanCongAsync(int maKyThuatVien, bool chiDangHieuLuc) =>
        _context.PhanCongXuLys.CountAsync(p =>
            p.MaKyThuatVien == maKyThuatVien
            && (!chiDangHieuLuc || p.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
        );

    public Task<TaiKhoan?> LayTaiKhoanAsync(int maTaiKhoan) =>
        _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

    public Task<bool> TaiKhoanDaGanHoSoKhacAsync(int maTaiKhoan, int? boQuaMaKyThuatVien = null) =>
        _dbSet.AnyAsync(k =>
            k.MaTaiKhoan == maTaiKhoan
            && (boQuaMaKyThuatVien == null || k.MaKyThuatVien != boQuaMaKyThuatVien)
        );

    public async Task<List<(int Ma, string Ten)>> LayTaiKhoanChoChonAsync(int? giuLaiMa)
    {
        var ds = await _context
            .TaiKhoans.AsNoTracking()
            .Where(t =>
                t.MaTaiKhoan == giuLaiMa
                || (
                    t.VaiTro == VaiTroEnum.KyThuatVien
                    && t.TrangThai
                    && t.KyThuatVien == null
                )
            )
            .OrderBy(t => t.HoTen)
            .Select(t => new { t.MaTaiKhoan, t.HoTen, t.TenDangNhap })
            .ToListAsync();
        return ds.Select(x => (x.MaTaiKhoan, $"{x.HoTen} ({x.TenDangNhap})")).ToList();
    }

    public async Task<List<(int Ma, string Ten)>> LayDangHoatDongAsync()
    {
        var ds = await QueryNoTracking()
            .Where(k => k.TrangThai)
            .OrderBy(k => k.HoTen)
            .Select(k => new { k.MaKyThuatVien, k.HoTen, k.ChuyenMon })
            .ToListAsync();
        return ds.Select(x => (
                x.MaKyThuatVien,
                string.IsNullOrEmpty(x.ChuyenMon) ? x.HoTen : $"{x.HoTen} - {x.ChuyenMon}"
            ))
            .ToList();
    }
}
