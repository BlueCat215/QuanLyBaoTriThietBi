// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Repository Phiếu xử lý và Phân công (LINQ phân trang, lọc, KTV hiện tại, đổi KTV giữ lịch sử)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class PhieuXuLyRepository : Repository<PhieuXuLy>, IPhieuXuLyRepository
{
    private const int SoTuKhoaToiDa = 5;

    public PhieuXuLyRepository(AppDbContext context) : base(context) { }

    private IQueryable<PhieuXuLy> BuildQuery(PhieuXuLyBoLoc boLoc)
    {
        IQueryable<PhieuXuLy> query = QueryNoTracking();

        if (boLoc.LoaiXuLy.HasValue)
            query = query.Where(p => p.LoaiXuLy == boLoc.LoaiXuLy.Value);

        if (boLoc.TrangThai.HasValue)
            query = query.Where(p => p.TrangThai == boLoc.TrangThai.Value);

        if (!boLoc.KhoangNgayKhongHopLe)
        {
            if (boLoc.TuNgay.HasValue)
                query = query.Where(p => p.NgayTao >= boLoc.TuNgay.Value.Date);

            if (boLoc.DenNgay.HasValue)
                query = query.Where(p => p.NgayTao < boLoc.DenNgay.Value.Date.AddDays(1));
        }

        if (boLoc.MaKyThuatVien.HasValue)
        {
            query = query.Where(p => p.PhanCongXuLys.Any(pc =>
                pc.MaKyThuatVien == boLoc.MaKyThuatVien.Value && pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc));
        }

        if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
        {
            var cacTu = boLoc.TuKhoa
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(SoTuKhoaToiDa);

            foreach (var tu in cacTu)
            {
                query = query.Where(p =>
                    (p.BaoCaoSuCo != null && (p.BaoCaoSuCo.ThietBi!.TenThietBi.Contains(tu) || p.BaoCaoSuCo.ThietBi.SoSerial.Contains(tu) || p.BaoCaoSuCo.MoTaSuCo.Contains(tu)))
                    || (p.KeHoachBaoTri != null && (p.KeHoachBaoTri.ThietBi!.TenThietBi.Contains(tu) || p.KeHoachBaoTri.ThietBi.SoSerial.Contains(tu) || p.KeHoachBaoTri.NoiDungBaoTri.Contains(tu)))
                    || (p.KetQuaXuLy != null && p.KetQuaXuLy.Contains(tu))
                    || p.PhanCongXuLys.Any(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc && pc.KyThuatVien!.HoTen.Contains(tu))
                );
            }
        }

        return query;
    }

    public async Task<PagedResult<PhieuXuLyDanhSachRow>> LayPhanTrangAsync(PhieuXuLyBoLoc boLoc)
    {
        var query = BuildQuery(boLoc);
        return await ExecutePagedQueryAsync(query, boLoc.Page, boLoc.PageSize);
    }

    public async Task<PagedResult<PhieuXuLyDanhSachRow>> LayPhanTrangTheoKtvAsync(int maKyThuatVien, PhieuXuLyBoLoc boLoc)
    {
        boLoc.MaKyThuatVien = maKyThuatVien;
        var query = BuildQuery(boLoc);
        return await ExecutePagedQueryAsync(query, boLoc.Page, boLoc.PageSize);
    }

    private async Task<PagedResult<PhieuXuLyDanhSachRow>> ExecutePagedQueryAsync(IQueryable<PhieuXuLy> query, int page, int pageSize)
    {
        var total = await query.CountAsync();
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 5, 100);

        var selectQuery = query
            .OrderByDescending(p => p.NgayTao)
            .ThenByDescending(p => p.MaPhieuXuLy)
            .Select(p => new
            {
                p.MaPhieuXuLy,
                p.LoaiXuLy,
                p.TrangThai,
                p.MaBaoCao,
                p.MaKeHoach,
                TenThietBi = p.LoaiXuLy == LoaiXuLyEnum.XuLySuCo
                    ? (p.BaoCaoSuCo != null ? p.BaoCaoSuCo.ThietBi!.TenThietBi : "")
                    : (p.KeHoachBaoTri != null ? p.KeHoachBaoTri.ThietBi!.TenThietBi : ""),
                SoSerial = p.LoaiXuLy == LoaiXuLyEnum.XuLySuCo
                    ? (p.BaoCaoSuCo != null ? p.BaoCaoSuCo.ThietBi!.SoSerial : "")
                    : (p.KeHoachBaoTri != null ? p.KeHoachBaoTri.ThietBi!.SoSerial : ""),
                TenKhuVuc = p.LoaiXuLy == LoaiXuLyEnum.XuLySuCo
                    ? (p.BaoCaoSuCo != null && p.BaoCaoSuCo.ThietBi != null && p.BaoCaoSuCo.ThietBi.KhuVuc != null ? p.BaoCaoSuCo.ThietBi.KhuVuc.TenKhuVuc : null)
                    : (p.KeHoachBaoTri != null && p.KeHoachBaoTri.ThietBi != null && p.KeHoachBaoTri.ThietBi.KhuVuc != null ? p.KeHoachBaoTri.ThietBi.KhuVuc.TenKhuVuc : null),
                PhanCongHienTai = p.PhanCongXuLys
                    .Where(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                    .Select(pc => new { pc.MaKyThuatVien, TenKtv = pc.KyThuatVien!.HoTen })
                    .FirstOrDefault(),
                p.NgayTao,
                p.NgayBatDau,
                p.NgayHoanThanh,
                TongChiPhi = p.ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m)
            });

        var rawList = await selectQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var rows = rawList.Select(x => new PhieuXuLyDanhSachRow(
            x.MaPhieuXuLy,
            x.LoaiXuLy,
            x.TrangThai,
            x.MaBaoCao,
            x.MaKeHoach,
            x.TenThietBi,
            x.SoSerial,
            x.TenKhuVuc,
            x.PhanCongHienTai?.MaKyThuatVien,
            x.PhanCongHienTai?.TenKtv,
            x.NgayTao,
            x.NgayBatDau,
            x.NgayHoanThanh,
            x.TongChiPhi
        )).ToList();

        return new PagedResult<PhieuXuLyDanhSachRow>
        {
            Items = rows,
            TotalItems = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<PhieuXuLy?> LayChiTietAsync(int maPhieuXuLy) =>
        _context.PhieuXuLys
            .Include(p => p.BaoCaoSuCo)
                .ThenInclude(b => b!.ThietBi)
                    .ThenInclude(t => t!.LoaiThietBi)
            .Include(p => p.BaoCaoSuCo)
                .ThenInclude(b => b!.ThietBi)
                    .ThenInclude(t => t!.KhuVuc)
            .Include(p => p.BaoCaoSuCo)
                .ThenInclude(b => b!.TaiKhoanBaoCao)
            .Include(p => p.KeHoachBaoTri)
                .ThenInclude(k => k!.ThietBi)
                    .ThenInclude(t => t!.LoaiThietBi)
            .Include(p => p.KeHoachBaoTri)
                .ThenInclude(k => k!.ThietBi)
                    .ThenInclude(t => t!.KhuVuc)
            .Include(p => p.ChiTietXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == maPhieuXuLy);

    /// STT 36: LINQ truy vấn KTV đang phụ trách hiện tại của 1 phiếu
    public Task<KyThuatVien?> LayKtvHienTaiAsync(int maPhieuXuLy) =>
        _context.PhanCongXuLys
            .AsNoTracking()
            .Where(pc => pc.MaPhieuXuLy == maPhieuXuLy && pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
            .Select(pc => pc.KyThuatVien)
            .FirstOrDefaultAsync();

    /// STT 36: Lấy bản ghi phân công hiện tại
    public Task<PhanCongXuLy?> LayPhanCongHienTaiAsync(int maPhieuXuLy) =>
        _context.PhanCongXuLys
            .FirstOrDefaultAsync(pc => pc.MaPhieuXuLy == maPhieuXuLy && pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc);

    /// STT 36: Lấy toàn bộ lịch sử phân công
    public async Task<List<PhanCongXuLyLichSuRow>> LayLichSuPhanCongAsync(int maPhieuXuLy) =>
        await _context.PhanCongXuLys
            .AsNoTracking()
            .Where(pc => pc.MaPhieuXuLy == maPhieuXuLy)
            .OrderByDescending(pc => pc.NgayPhanCong)
            .ThenByDescending(pc => pc.MaPhanCong)
            .Select(pc => new PhanCongXuLyLichSuRow(
                pc.MaPhanCong,
                pc.MaKyThuatVien,
                pc.KyThuatVien!.HoTen,
                pc.KyThuatVien.ChuyenMon,
                pc.KyThuatVien.SoDienThoai,
                pc.KyThuatVien.Email,
                pc.NgayPhanCong,
                pc.NgayKetThucPhanCong,
                pc.NoiDungPhanCong,
                pc.TrangThai
            ))
            .ToListAsync();

    public async Task ThemPhanCongAsync(PhanCongXuLy phanCong) =>
        await _context.PhanCongXuLys.AddAsync(phanCong);

    public Task<bool> KtvDangHoatDongAsync(int maKyThuatVien) =>
        _context.KyThuatViens.AnyAsync(k => k.MaKyThuatVien == maKyThuatVien && k.TrangThai);

    public Task<bool> DaCoPhieuDangHoatDongChoBaoCaoAsync(int maBaoCao) =>
        _context.PhieuXuLys.AnyAsync(p =>
            p.MaBaoCao == maBaoCao
            && p.TrangThai != TrangThaiPhieuXuLyEnum.HoanThanh
            && p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy
        );

    public Task<bool> DaCoPhieuDangHoatDongChoKeHoachAsync(int maKeHoach) =>
        _context.PhieuXuLys.AnyAsync(p =>
            p.MaKeHoach == maKeHoach
            && p.TrangThai != TrangThaiPhieuXuLyEnum.HoanThanh
            && p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy
        );

    public async Task<List<(int Ma, string MoTa)>> LayBaoCaoChoChonAsync()
    {
        var query = from b in _context.BaoCaoSuCos.AsNoTracking()
                    where b.TrangThai == TrangThaiBaoCaoEnum.MoiBao || b.TrangThai == TrangThaiBaoCaoEnum.DaTiepNhan
                    orderby b.NgayBaoCao descending
                    select new
                    {
                        b.MaBaoCao,
                        b.MoTaSuCo,
                        TenThietBi = b.ThietBi!.TenThietBi,
                        b.MucDoSuCo
                    };

        var list = await query.Take(50).ToListAsync();
        return list.Select(x => (x.MaBaoCao, $"{UiHelper.Ma("SC", x.MaBaoCao)} - {x.TenThietBi} ({UiHelper.Badge(x.MucDoSuCo).Text}): {x.MoTaSuCo}")).ToList();
    }

    public async Task<List<(int Ma, string MoTa)>> LayKeHoachChoChonAsync()
    {
        var query = from k in _context.KeHoachBaoTris.AsNoTracking()
                    where k.TrangThai == TrangThaiKeHoachEnum.ChoThucHien || k.TrangThai == TrangThaiKeHoachEnum.DaPhanCong
                    orderby k.NgayDuKien
                    select new
                    {
                        k.MaKeHoach,
                        k.NoiDungBaoTri,
                        TenThietBi = k.ThietBi!.TenThietBi,
                        k.NgayDuKien
                    };

        var list = await query.Take(50).ToListAsync();
        return list.Select(x => (x.MaKeHoach, $"{UiHelper.Ma("KH", x.MaKeHoach)} - {x.TenThietBi} (Dự kiến: {UiHelper.Ngay(x.NgayDuKien)}): {x.NoiDungBaoTri}")).ToList();
    }

    public async Task<List<(int Ma, string Ten)>> LayKtvDangHoatDongChoChonAsync() =>
        await _context.KyThuatViens
            .AsNoTracking()
            .Where(k => k.TrangThai)
            .OrderBy(k => k.HoTen)
            .Select(k => new ValueTuple<int, string>(k.MaKyThuatVien, k.HoTen + (k.ChuyenMon != null ? $" ({k.ChuyenMon})" : "")))
            .ToListAsync();

    public Task<KyThuatVien?> LayKtvTheoTaiKhoanAsync(int maTaiKhoan) =>
        _context.KyThuatViens
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);
}
