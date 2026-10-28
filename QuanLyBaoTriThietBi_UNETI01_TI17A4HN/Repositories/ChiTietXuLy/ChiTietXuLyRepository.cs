// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Repository Chi tiết xử lý (LINQ: danh sách phiếu, chi tiết theo phiếu, tổng chi phí = SUM(ThanhTien), hỗ trợ phân quyền và bắt đầu xử lý)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public class ChiTietXuLyRepository : Repository<ChiTietXuLy>, IChiTietXuLyRepository
{
    public ChiTietXuLyRepository(AppDbContext context)
        : base(context) { }

    public async Task<List<PhieuXuLyRow>> LayDanhSachPhieuAsync(TrangThaiPhieuXuLyEnum? trangThai)
    {
        IQueryable<PhieuXuLy> query = _context.PhieuXuLys.AsNoTracking();

        if (trangThai is { } tt)
            query = query.Where(p => p.TrangThai == tt);

        // TongChiPhi tính bằng SUM trong SQL (thuộc tính PhieuXuLy.TongChiPhi là [NotMapped], không dùng được trong truy vấn)
        var ds = await query
            .OrderByDescending(p => p.NgayTao)
            .ThenByDescending(p => p.MaPhieuXuLy)
            .Select(p => new
            {
                p.MaPhieuXuLy,
                p.LoaiXuLy,
                p.TrangThai,
                p.NgayTao,
                TenThietBi = p.BaoCaoSuCo != null
                    ? p.BaoCaoSuCo.ThietBi!.TenThietBi
                    : (p.KeHoachBaoTri != null ? p.KeHoachBaoTri.ThietBi!.TenThietBi : null),
                SoSerial = p.BaoCaoSuCo != null
                    ? p.BaoCaoSuCo.ThietBi!.SoSerial
                    : (p.KeHoachBaoTri != null ? p.KeHoachBaoTri.ThietBi!.SoSerial : null),
                SoDong = p.ChiTietXuLys.Count(),
                Tong = p.ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m),
            })
            .ToListAsync();

        return ds.Select(x => new PhieuXuLyRow(
                x.MaPhieuXuLy,
                x.LoaiXuLy,
                x.TrangThai,
                x.TenThietBi,
                x.SoSerial,
                x.NgayTao,
                x.SoDong,
                x.Tong
            ))
            .ToList();
    }

    public async Task<PhieuTomTat?> LayPhieuAsync(int maPhieuXuLy)
    {
        var x = await _context
            .PhieuXuLys.AsNoTracking()
            .Where(p => p.MaPhieuXuLy == maPhieuXuLy)
            .Select(p => new
            {
                p.MaPhieuXuLy,
                p.LoaiXuLy,
                p.TrangThai,
                p.MaBaoCao,
                p.MaKeHoach,
                p.NgayTao,
                p.NgayBatDau,
                p.NgayHoanThanh,
                p.KetQuaXuLy,
                TenThietBi = p.BaoCaoSuCo != null
                    ? p.BaoCaoSuCo.ThietBi!.TenThietBi
                    : (p.KeHoachBaoTri != null ? p.KeHoachBaoTri.ThietBi!.TenThietBi : null),
                SoSerial = p.BaoCaoSuCo != null
                    ? p.BaoCaoSuCo.ThietBi!.SoSerial
                    : (p.KeHoachBaoTri != null ? p.KeHoachBaoTri.ThietBi!.SoSerial : null),
                Ktv = p.PhanCongXuLys
                    .Where(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                    .Select(pc => new { pc.MaKyThuatVien, Ten = pc.KyThuatVien != null ? pc.KyThuatVien.HoTen : null })
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync();

        return x is null
            ? null
            : new PhieuTomTat(
                x.MaPhieuXuLy,
                x.LoaiXuLy,
                x.TrangThai,
                x.MaBaoCao,
                x.MaKeHoach,
                x.NgayTao,
                x.NgayBatDau,
                x.NgayHoanThanh,
                x.KetQuaXuLy,
                x.TenThietBi,
                x.SoSerial,
                x.Ktv?.MaKyThuatVien,
                x.Ktv?.Ten
            );
    }

    public Task<List<ChiTietXuLyRow>> LayTheoPhieuAsync(int maPhieuXuLy) =>
        QueryNoTracking()
            .Where(c => c.MaPhieuXuLy == maPhieuXuLy)
            .OrderBy(c => c.NgayCapNhat)
            .ThenBy(c => c.MaChiTiet)
            .Select(c => new ChiTietXuLyRow(
                c.MaChiTiet,
                c.NgayCapNhat,
                c.NoiDungThucHien,
                c.VatTuLinhKien,
                c.SoLuong,
                c.DonGia,
                c.ThanhTien,
                c.GhiChu
            ))
            .ToListAsync();

    public Task<decimal> TongChiPhiAsync(int maPhieuXuLy) =>
        QueryNoTracking()
            .Where(c => c.MaPhieuXuLy == maPhieuXuLy)
            .SumAsync(c => c.ThanhTien ?? 0m);

    public Task<VatTu?> LayVatTuAsync(int maVatTu) =>
        _context.VatTus.AsNoTracking().FirstOrDefaultAsync(v => v.MaVatTu == maVatTu);

    public async Task<List<VatTuLuaChon>> LayVatTuChoChonAsync(int? giuLaiMa)
    {
        var ds = await _context
            .VatTus.AsNoTracking()
            .Where(v => v.TrangThai || v.MaVatTu == giuLaiMa)
            .OrderBy(v => v.TenVatTu)
            .Select(v => new
            {
                v.MaVatTu,
                v.TenVatTu,
                v.DonViTinh,
                v.DonGiaMacDinh,
            })
            .ToListAsync();

        return ds.Select(v => new VatTuLuaChon(v.MaVatTu, v.TenVatTu, v.DonViTinh, v.DonGiaMacDinh))
            .ToList();
    }

    public Task<KyThuatVien?> LayKtvTheoTaiKhoanAsync(int maTaiKhoan) =>
        _context.KyThuatViens.AsNoTracking().FirstOrDefaultAsync(k => k.MaTaiKhoan == maTaiKhoan);

    public Task<PhanCongXuLy?> LayPhanCongHienTaiAsync(int maPhieuXuLy) =>
        _context.PhanCongXuLys.AsNoTracking()
            .FirstOrDefaultAsync(pc => pc.MaPhieuXuLy == maPhieuXuLy && pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc);

    public Task<PhieuXuLy?> LayPhieuEntityAsync(int maPhieuXuLy) =>
        _context.PhieuXuLys
            .Include(p => p.BaoCaoSuCo).ThenInclude(b => b!.ThietBi)
            .Include(p => p.KeHoachBaoTri).ThenInclude(k => k!.ThietBi)
            .Include(p => p.PhanCongXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == maPhieuXuLy);
}
