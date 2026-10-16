// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Repository Báo cáo sự cố (LINQ: tìm kiếm + lọc, đếm dữ liệu liên quan)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class BaoCaoSuCoRepository : Repository<BaoCaoSuCo>, IBaoCaoSuCoRepository
{
    private const int SoTuKhoaToiDa = 5;

    public BaoCaoSuCoRepository(AppDbContext context)
        : base(context) { }

    public async Task<List<BaoCaoSuCoRow>> TimKiemAsync(
        string? tuKhoa,
        TrangThaiBaoCaoEnum? trangThai,
        MucDoSuCoEnum? mucDo,
        int? maTaiKhoan
    )
    {
        IQueryable<BaoCaoSuCo> query = QueryNoTracking();

        if (maTaiKhoan is { } mk)
            query = query.Where(b => b.MaTaiKhoanBaoCao == mk);
        if (trangThai is { } tt)
            query = query.Where(b => b.TrangThai == tt);
        if (mucDo is { } md)
            query = query.Where(b => b.MucDoSuCo == md);

        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            var cacTu = tuKhoa
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Take(SoTuKhoaToiDa);

            foreach (var tu in cacTu)
            {
                query = query.Where(b =>
                    b.ThietBi!.TenThietBi.Contains(tu)
                    || b.ThietBi.SoSerial.Contains(tu)
                    || b.MoTaSuCo.Contains(tu)
                    || b.TaiKhoanBaoCao!.HoTen.Contains(tu)
                );
            }
        }

        var ds = await query
            .OrderByDescending(b => b.NgayBaoCao)
            .ThenByDescending(b => b.MaBaoCao)
            .Select(b => new
            {
                b.MaBaoCao,
                b.MaThietBi,
                TenThietBi = b.ThietBi!.TenThietBi,
                SoSerial = b.ThietBi.SoSerial,
                NguoiBao = b.TaiKhoanBaoCao!.HoTen,
                b.NgayBaoCao,
                b.MoTaSuCo,
                b.MucDoSuCo,
                b.TrangThai,
            })
            .ToListAsync();

        return ds.Select(x => new BaoCaoSuCoRow(
                x.MaBaoCao,
                x.MaThietBi,
                x.TenThietBi,
                x.SoSerial,
                x.NguoiBao,
                x.NgayBaoCao,
                x.MoTaSuCo,
                x.MucDoSuCo,
                x.TrangThai
            ))
            .ToList();
    }

    public Task<BaoCaoSuCo?> LayChiTietAsync(int maBaoCao) =>
        QueryNoTracking()
            .Include(b => b.ThietBi!)
                .ThenInclude(t => t.KhuVuc)
            .Include(b => b.ThietBi!)
                .ThenInclude(t => t.LoaiThietBi)
            .Include(b => b.TaiKhoanBaoCao)
            .FirstOrDefaultAsync(b => b.MaBaoCao == maBaoCao);

    public async Task<List<PhieuXuLyTomTat>> LayPhieuXuLyAsync(int maBaoCao)
    {
        var ds = await _context
            .PhieuXuLys.AsNoTracking()
            .Where(p => p.MaBaoCao == maBaoCao)
            .OrderByDescending(p => p.NgayTao)
            .Select(p => new { p.MaPhieuXuLy, p.LoaiXuLy, p.TrangThai, p.NgayTao })
            .ToListAsync();
        return ds.Select(x => new PhieuXuLyTomTat(x.MaPhieuXuLy, x.LoaiXuLy, x.TrangThai, x.NgayTao))
            .ToList();
    }

    public async Task<DuLieuLienQuanBaoCao> DemDuLieuLienQuanAsync(int maBaoCao)
    {
        var soPhieu = await _context.PhieuXuLys.CountAsync(p => p.MaBaoCao == maBaoCao);
        var soPhieuConHieuLuc = await _context.PhieuXuLys.CountAsync(p =>
            p.MaBaoCao == maBaoCao && p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy
        );
        var soTep = await _context.TepDinhKems.CountAsync(f =>
            f.LoaiDoiTuong == LoaiDoiTuongDinhKemEnum.BaoCaoSuCo && f.MaDoiTuong == maBaoCao
        );
        return new DuLieuLienQuanBaoCao(soPhieu, soPhieuConHieuLuc, soTep);
    }

    public Task<ThietBi?> LayThietBiAsync(int maThietBi) =>
        _context.ThietBis.AsNoTracking().FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);

    public Task<TaiKhoan?> LayTaiKhoanAsync(int maTaiKhoan) =>
        _context.TaiKhoans.AsNoTracking().FirstOrDefaultAsync(t => t.MaTaiKhoan == maTaiKhoan);

    public async Task<int?> LayMaTaiKhoanNguoiDungDauTienAsync() =>
        await _context
            .TaiKhoans.AsNoTracking()
            .Where(t => t.VaiTro == VaiTroEnum.NguoiSuDung && t.TrangThai)
            .OrderBy(t => t.MaTaiKhoan)
            .Select(t => (int?)t.MaTaiKhoan)
            .FirstOrDefaultAsync();

    public async Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(
        bool gioiHanKhuVuc,
        int? maKhuVuc,
        int? giuLaiMa
    )
    {
        var ds = await _context
            .ThietBis.AsNoTracking()
            .Where(t =>
                t.MaThietBi == giuLaiMa
                || (
                    t.TinhTrang != TinhTrangThietBiEnum.NgungSuDung
                    && (!gioiHanKhuVuc || t.MaKhuVuc == maKhuVuc)
                )
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
}
