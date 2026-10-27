// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Repository Kế hoạch bảo trì (LINQ: tìm kiếm + lọc, chống trùng kế hoạch cùng kỳ, nạp chi tiết vòng đời)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class KeHoachBaoTriRepository : Repository<KeHoachBaoTri>, IKeHoachBaoTriRepository
{
    private const int SoTuKhoaToiDa = 5;

    public KeHoachBaoTriRepository(AppDbContext context)
        : base(context) { }

    public async Task<List<KeHoachRow>> TimKiemAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    )
    {
        IQueryable<KeHoachBaoTri> query = QueryNoTracking();

        if (trangThai is { } tt)
            query = query.Where(k => k.TrangThai == tt);
        if (mucDo is { } md)
            query = query.Where(k => k.MucDoUuTien == md);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var cacTu = tuKhoa
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(SoTuKhoaToiDa);

            foreach (var tu in cacTu)
            {
                query = query.Where(k =>
                    k.ThietBi!.TenThietBi.Contains(tu)
                    || k.ThietBi.SoSerial.Contains(tu)
                    || k.ThietBi.KhuVuc!.TenKhuVuc.Contains(tu)
                    || k.NoiDungBaoTri.Contains(tu)
                );
            }
        }

        var ds = await query
            // Kế hoạch đã Hoàn thành/Đã hủy xuống cuối, còn lại theo ngày dự kiến gần nhất
            .OrderBy(k =>
                k.TrangThai == TrangThaiKeHoachEnum.HoanThanh
                || k.TrangThai == TrangThaiKeHoachEnum.DaHuy
                    ? 1
                    : 0
            )
            .ThenBy(k => k.NgayDuKien)
            .ThenBy(k => k.MaKeHoach)
            .Select(k => new
            {
                k.MaKeHoach,
                k.MaThietBi,
                TenThietBi = k.ThietBi!.TenThietBi,
                SoSerial = k.ThietBi.SoSerial,
                TenKhuVuc = k.ThietBi.KhuVuc!.TenKhuVuc,
                k.NgayDuKien,
                k.NoiDungBaoTri,
                k.MucDoUuTien,
                k.TrangThai,
            })
            .ToListAsync();

        return ds.Select(x => new KeHoachRow(
                x.MaKeHoach,
                x.MaThietBi,
                x.TenThietBi,
                x.SoSerial,
                x.TenKhuVuc,
                x.NgayDuKien,
                x.NoiDungBaoTri,
                x.MucDoUuTien,
                x.TrangThai
            ))
            .ToList();
    }

    public Task<KeHoachBaoTri?> LayChiTietTheoIdAsync(int id) =>
        _context.KeHoachBaoTris
            .Include(k => k.ThietBi)
                .ThenInclude(t => t!.LoaiThietBi)
            .Include(k => k.ThietBi)
                .ThenInclude(t => t!.KhuVuc)
            .Include(k => k.PhieuXuLys)
                .ThenInclude(p => p.PhanCongXuLys)
                    .ThenInclude(pc => pc.KyThuatVien)
            .FirstOrDefaultAsync(k => k.MaKeHoach == id);

    public Task<ThietBi?> LayThietBiAsync(int maThietBi) =>
        _context.ThietBis.AsNoTracking().FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);

    public Task<ThietBi?> LayThietBiCoTheoDoiAsync(int maThietBi) =>
        _context.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);

    public async Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(int? giuLaiMa)
    {
        var ds = await _context
            .ThietBis.AsNoTracking()
            .Where(t =>
                t.MaThietBi == giuLaiMa || t.TinhTrang != TinhTrangThietBiEnum.NgungSuDung
            )
            .OrderBy(t => t.TenThietBi)
            .Select(t => new
            {
                t.MaThietBi,
                t.TenThietBi,
                t.SoSerial,
                TenKhuVuc = t.KhuVuc!.TenKhuVuc,
            })
            .ToListAsync();
        return ds.Select(x => (x.MaThietBi, $"{x.TenThietBi} ({x.SoSerial}) · {x.TenKhuVuc}"))
            .ToList();
    }

    public Task<KeHoachBaoTri?> TimKeHoachTrungKyAsync(
        int maThietBi,
        DateTime ngayDuKien,
        int? boQuaMaKeHoach = null
    )
    {
        // Kỳ bảo trì = tháng-năm của ngày dự kiến: [ngày 1 của tháng, ngày 1 của tháng sau)
        var dauKy = new DateTime(ngayDuKien.Year, ngayDuKien.Month, 1);
        var cuoiKy = dauKy.AddMonths(1);

        return QueryNoTracking()
            .OrderBy(k => k.NgayDuKien)
            .FirstOrDefaultAsync(k =>
                k.MaThietBi == maThietBi
                && k.NgayDuKien >= dauKy
                && k.NgayDuKien < cuoiKy
                && k.TrangThai != TrangThaiKeHoachEnum.HoanThanh
                && k.TrangThai != TrangThaiKeHoachEnum.DaHuy
                && (boQuaMaKeHoach == null || k.MaKeHoach != boQuaMaKeHoach)
            );
    }

    public Task<int> DemPhieuConHieuLucAsync(int maKeHoach) =>
        _context.PhieuXuLys.CountAsync(p =>
            p.MaKeHoach == maKeHoach && p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy
        );
}
