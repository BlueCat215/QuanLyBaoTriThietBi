// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Repository Kế hoạch bảo trì (LINQ: tìm kiếm + lọc ngày/ưu tiên/trạng thái, sắp xếp, phân trang, chống trùng kỳ, liên kết Phiếu xử lý, nạp chi tiết vòng đời)

#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class KeHoachBaoTriRepository : Repository<KeHoachBaoTri>, IKeHoachBaoTriRepository
{
    private const int SoTuKhoaToiDa = 5;

    public KeHoachBaoTriRepository(AppDbContext context)
        : base(context) { }

    /// Lấy danh sách kế hoạch có phân trang bằng Skip/Take (thứ tự: Where -> OrderBy -> Skip -> Take)
    public async Task<PagedResult<KeHoachRow>> LayPhanTrangAsync(KeHoachBaoTriBoLoc boLoc)
    {
        IQueryable<KeHoachBaoTri> query = QueryNoTracking();

        // 1. Where - Bộ lọc
        if (boLoc.TrangThai is { } tt)
            query = query.Where(k => k.TrangThai == tt);

        if (boLoc.MucDo is { } md)
            query = query.Where(k => k.MucDoUuTien == md);

        if (!boLoc.KhoangNgayKhongHopLe)
        {
            if (boLoc.TuNgay is { } tu)
                query = query.Where(k => k.NgayDuKien >= tu.Date);

            if (boLoc.DenNgay is { } den)
                query = query.Where(k => k.NgayDuKien <= den.Date);
        }

        if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
        {
            var cacTu = boLoc.TuKhoa
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

        // 2. OrderBy - Sắp xếp linh hoạt
        query = boLoc.SapXep switch
        {
            KeHoachBaoTriSapXep.NgayDuKienXaNhat => query
                .OrderByDescending(k => k.NgayDuKien)
                .ThenByDescending(k => k.MaKeHoach),

            KeHoachBaoTriSapXep.UuTienCaoNhat => query
                .OrderByDescending(k => k.MucDoUuTien)
                .ThenBy(k => k.NgayDuKien)
                .ThenBy(k => k.MaKeHoach),

            KeHoachBaoTriSapXep.UuTienThapNhat => query
                .OrderBy(k => k.MucDoUuTien)
                .ThenBy(k => k.NgayDuKien)
                .ThenBy(k => k.MaKeHoach),

            KeHoachBaoTriSapXep.TenThietBiAZ => query
                .OrderBy(k => k.ThietBi!.TenThietBi)
                .ThenBy(k => k.NgayDuKien)
                .ThenBy(k => k.MaKeHoach),

            KeHoachBaoTriSapXep.TenThietBiZA => query
                .OrderByDescending(k => k.ThietBi!.TenThietBi)
                .ThenBy(k => k.NgayDuKien)
                .ThenBy(k => k.MaKeHoach),

            KeHoachBaoTriSapXep.MaKeHoachMoiNhat => query
                .OrderByDescending(k => k.MaKeHoach),

            _ => query
                // Mặc định: Kế hoạch đã Hoàn thành/Đã hủy xuống cuối, còn lại theo ngày dự kiến gần nhất
                .OrderBy(k =>
                    k.TrangThai == TrangThaiKeHoachEnum.HoanThanh
                    || k.TrangThai == TrangThaiKeHoachEnum.DaHuy
                        ? 1
                        : 0
                )
                .ThenBy(k => k.NgayDuKien)
                .ThenBy(k => k.MaKeHoach),
        };

        // 3. Phân trang
        var page = boLoc.Trang < 1 ? 1 : boLoc.Trang;
        var pageSize = boLoc.SoBanGhiMoiTrang < 1 ? 10 : boLoc.SoBanGhiMoiTrang;
        var total = await query.CountAsync();

        var totalPages = pageSize <= 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        if (page > totalPages && totalPages > 0)
            page = totalPages;

        var ds = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
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
                SoPhieuXuLy = k.PhieuXuLys.Count,
            })
            .ToListAsync();

        var items = ds.Select(x => new KeHoachRow(
            x.MaKeHoach,
            x.MaThietBi,
            x.TenThietBi,
            x.SoSerial,
            x.TenKhuVuc,
            x.NgayDuKien,
            x.NoiDungBaoTri,
            x.MucDoUuTien,
            x.TrangThai,
            x.SoPhieuXuLy
        )).ToList();

        return new PagedResult<KeHoachRow>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = total,
        };
    }

    public async Task<List<KeHoachRow>> TimKiemAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    )
    {
        var boLoc = new KeHoachBaoTriBoLoc
        {
            TuKhoa = tuKhoa,
            TrangThai = trangThai,
            MucDo = mucDo,
            SoBanGhiMoiTrang = 1000,
        };
        var paged = await LayPhanTrangAsync(boLoc);
        return paged.Items;
    }

    public Task<KeHoachBaoTri?> LayChiTietAsync(int maKeHoach) =>
        QueryNoTracking()
            .Include(k => k.ThietBi!)
                .ThenInclude(t => t.LoaiThietBi)
            .Include(k => k.ThietBi!)
                .ThenInclude(t => t.KhuVuc)
            .FirstOrDefaultAsync(k => k.MaKeHoach == maKeHoach);

    public async Task<List<PhieuXuLyLienKetRow>> LayPhieuXuLyTheoKeHoachAsync(int maKeHoach)
    {
        var ds = await _context.PhieuXuLys
            .AsNoTracking()
            .Where(p => p.MaKeHoach == maKeHoach)
            .OrderByDescending(p => p.NgayTao)
            .Select(p => new
            {
                p.MaPhieuXuLy,
                p.LoaiXuLy,
                p.TrangThai,
                p.NgayTao,
                p.NgayBatDau,
                p.NgayHoanThanh,
                TenKyThuatVien = p.PhanCongXuLys
                    .Where(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                    .Select(pc => pc.KyThuatVien!.HoTen)
                    .FirstOrDefault(),
                p.KetQuaXuLy,
                TongChiPhi = p.ChiTietXuLys.Sum(ct => ct.ThanhTien ?? 0m),
            })
            .ToListAsync();

        return ds.Select(x => new PhieuXuLyLienKetRow(
            x.MaPhieuXuLy,
            x.LoaiXuLy,
            x.TrangThai,
            x.NgayTao,
            x.NgayBatDau,
            x.NgayHoanThanh,
            x.TenKyThuatVien,
            x.KetQuaXuLy,
            x.TongChiPhi
        )).ToList();
    }

    public Task<bool> CoPhieuDangHoatDongAsync(int maKeHoach) =>
        _context.PhieuXuLys.AnyAsync(p =>
            p.MaKeHoach == maKeHoach
            && p.TrangThai != TrangThaiPhieuXuLyEnum.HoanThanh
            && p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy
        );

    public async Task ThemPhieuXuLyAsync(PhieuXuLy phieu) =>
        await _context.PhieuXuLys.AddAsync(phieu);

    public Task<ThietBi?> LayThietBiAsync(int maThietBi) =>
        _context.ThietBis.FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);

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
