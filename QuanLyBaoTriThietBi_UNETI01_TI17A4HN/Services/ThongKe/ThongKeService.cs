// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Service Báo cáo thống kê (SST 45: LINQ GroupBy theo loại, khu vực, mức độ, KTV, chi phí & thời gian xử lý)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class ThongKeService : IThongKeService
{
    private readonly AppDbContext _context;

    public ThongKeService(AppDbContext context) => _context = context;

    public async Task<ThongKeIndexViewModel> LayThongKeAsync(ThongKeBoLoc boLoc)
    {
        // 1. Xác định mốc thời gian lọc
        DateTime startDate;
        DateTime endDate;
        int namChon = boLoc.Nam ?? DateTime.Today.Year;

        if (boLoc.TuNgay.HasValue || boLoc.DenNgay.HasValue)
        {
            startDate = boLoc.TuNgay?.Date ?? new DateTime(2000, 1, 1);
            endDate = (boLoc.DenNgay?.Date.AddDays(1)) ?? DateTime.MaxValue;
        }
        else
        {
            startDate = new DateTime(namChon, 1, 1);
            endDate = new DateTime(namChon + 1, 1, 1);
        }

        // 2. Tải toàn bộ dữ liệu cần thiết phục vụ thống kê (truy vấn NoTracking tối ưu hiệu năng)
        var thietBis = await _context.ThietBis
            .AsNoTracking()
            .Include(t => t.LoaiThietBi)
            .Include(t => t.KhuVuc)
            .ToListAsync();

        var loaiThietBis = await _context.LoaiThietBis.AsNoTracking().ToListAsync();
        var khuVucs = await _context.KhuVucs.AsNoTracking().ToListAsync();

        var baoCaos = await _context.BaoCaoSuCos
            .AsNoTracking()
            .Where(b => b.NgayBaoCao >= startDate && b.NgayBaoCao < endDate)
            .ToListAsync();

        var keHoachs = await _context.KeHoachBaoTris
            .AsNoTracking()
            .Where(k => k.NgayDuKien >= startDate && k.NgayDuKien < endDate)
            .ToListAsync();

        var phieus = await _context.PhieuXuLys
            .AsNoTracking()
            .Include(p => p.BaoCaoSuCo)
            .Include(p => p.KeHoachBaoTri)
            .Include(p => p.PhanCongXuLys)
                .ThenInclude(pc => pc.KyThuatVien)
            .Include(p => p.ChiTietXuLys)
            .Where(p => p.NgayTao >= startDate && p.NgayTao < endDate)
            .ToListAsync();

        var ktvs = await _context.KyThuatViens
            .AsNoTracking()
            .ToListAsync();

        // 3. Tính toán các chỉ số tổng quan
        int tongThietBi = thietBis.Count;
        int tongSuCo = baoCaos.Count;
        int tongKeHoach = keHoachs.Count;
        int tongPhieu = phieus.Count;
        int tongPhieuHoanThanh = phieus.Count(p => p.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh);

        decimal tongChiPhi = phieus
            .Where(p => p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy)
            .Sum(p => p.TongChiPhi);

        var phieuCoThoiGian = phieus
            .Where(p => p.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh
                     && p.NgayBatDau.HasValue
                     && p.NgayHoanThanh.HasValue
                     && p.NgayHoanThanh.Value >= p.NgayBatDau.Value)
            .Select(p => (p.NgayHoanThanh!.Value - p.NgayBatDau!.Value).TotalHours)
            .ToList();

        double tgXuLyTbGio = phieuCoThoiGian.Count > 0 ? Math.Round(phieuCoThoiGian.Average(), 1) : 0;

        // 4. GroupBy theo Loại thiết bị
        var theoLoai = loaiThietBis.Select(l =>
        {
            var dsTb = thietBis.Where(t => t.MaLoaiThietBi == l.MaLoaiThietBi).Select(t => t.MaThietBi).ToHashSet();
            int soTb = dsTb.Count;
            int soSc = baoCaos.Count(b => dsTb.Contains(b.MaThietBi));
            int soKh = keHoachs.Count(k => dsTb.Contains(k.MaThietBi));
            var phieuLoai = phieus.Where(p =>
            {
                int? tbId = p.BaoCaoSuCo?.MaThietBi ?? p.KeHoachBaoTri?.MaThietBi;
                return tbId.HasValue && dsTb.Contains(tbId.Value);
            }).ToList();

            decimal cp = phieuLoai.Where(p => p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy).Sum(p => p.TongChiPhi);

            return new ThongKeNhomItem(
                l.MaLoaiThietBi,
                l.TenLoaiThietBi,
                soTb,
                soSc,
                soKh,
                phieuLoai.Count,
                cp
            );
        }).OrderByDescending(x => x.TongChiPhi).ThenByDescending(x => x.SoThietBi).ToList();

        // 5. GroupBy theo Khu vực
        var theoKhuVuc = khuVucs.Select(k =>
        {
            var dsTb = thietBis.Where(t => t.MaKhuVuc == k.MaKhuVuc).Select(t => t.MaThietBi).ToHashSet();
            int soTb = dsTb.Count;
            int soSc = baoCaos.Count(b => dsTb.Contains(b.MaThietBi));
            int soKh = keHoachs.Count(k => dsTb.Contains(k.MaThietBi));
            var phieuKhuVuc = phieus.Where(p =>
            {
                int? tbId = p.BaoCaoSuCo?.MaThietBi ?? p.KeHoachBaoTri?.MaThietBi;
                return tbId.HasValue && dsTb.Contains(tbId.Value);
            }).ToList();

            decimal cp = phieuKhuVuc.Where(p => p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy).Sum(p => p.TongChiPhi);

            return new ThongKeNhomItem(
                k.MaKhuVuc,
                k.TenKhuVuc,
                soTb,
                soSc,
                soKh,
                phieuKhuVuc.Count,
                cp
            );
        }).OrderByDescending(x => x.TongChiPhi).ThenByDescending(x => x.SoThietBi).ToList();

        // 6. GroupBy theo Mức độ sự cố
        var theoMucDo = Enum.GetValues<MucDoSuCoEnum>().Select(m =>
        {
            var ds = baoCaos.Where(b => b.MucDoSuCo == m).ToList();
            int count = ds.Count;
            int daXong = ds.Count(b => b.TrangThai == TrangThaiBaoCaoEnum.HoanThanh);
            double pct = tongSuCo > 0 ? Math.Round((double)count * 100 / tongSuCo, 1) : 0;
            return new ThongKeMucDoItem(
                m,
                UiHelper.Badge(m).Text,
                count,
                daXong,
                pct
            );
        }).ToList();

        // 7. GroupBy theo Kỹ thuật viên (Hiệu suất và số lượng phiếu)
        var theoKtv = ktvs.Select(k =>
        {
            // Các phiếu do KTV này từng hoặc đang phụ trách
            var phieuCuaKtv = phieus.Where(p =>
                p.PhanCongXuLys.Any(pc => pc.MaKyThuatVien == k.MaKyThuatVien)
            ).ToList();

            int phieuHt = phieuCuaKtv.Count(p => p.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh);
            int phieuDangXl = phieuCuaKtv.Count(p => p.TrangThai == TrangThaiPhieuXuLyEnum.DangXuLy);

            var tgList = phieuCuaKtv
                .Where(p => p.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh
                         && p.NgayBatDau.HasValue
                         && p.NgayHoanThanh.HasValue
                         && p.NgayHoanThanh.Value >= p.NgayBatDau.Value)
                .Select(p => (p.NgayHoanThanh!.Value - p.NgayBatDau!.Value).TotalHours)
                .ToList();

            double tgTb = tgList.Count > 0 ? Math.Round(tgList.Average(), 1) : 0;
            decimal chiPhiVatTu = phieuCuaKtv
                .Where(p => p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy)
                .Sum(p => p.TongChiPhi);

            return new ThongKeKtvItem(
                k.MaKyThuatVien,
                k.HoTen,
                k.ChuyenMon,
                k.SoDienThoai,
                phieuCuaKtv.Count,
                phieuHt,
                phieuDangXl,
                tgTb,
                chiPhiVatTu
            );
        }).OrderByDescending(k => k.SoPhieuHoanThanh).ThenByDescending(k => k.TongPhieuPhuTrach).ToList();

        // 8. Top 5 thiết bị nhiều sự cố nhất
        var scGroup = baoCaos
            .GroupBy(b => b.MaThietBi)
            .Select(g => new { MaThietBi = g.Key, SoSuCo = g.Count() })
            .OrderByDescending(x => x.SoSuCo)
            .Take(5)
            .ToList();

        var topNhieuSuCo = scGroup.Select(g =>
        {
            var tb = thietBis.FirstOrDefault(t => t.MaThietBi == g.MaThietBi);
            var phieuTb = phieus.Where(p => (p.BaoCaoSuCo?.MaThietBi ?? p.KeHoachBaoTri?.MaThietBi) == g.MaThietBi).ToList();
            decimal cp = phieuTb.Where(p => p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy).Sum(p => p.TongChiPhi);

            return new ThietBiNhieuSuCoItem(
                g.MaThietBi,
                tb?.TenThietBi ?? "—",
                tb?.SoSerial ?? "—",
                tb?.LoaiThietBi?.TenLoaiThietBi,
                tb?.KhuVuc?.TenKhuVuc,
                g.SoSuCo,
                phieuTb.Count,
                cp,
                tb?.TinhTrang ?? TinhTrangThietBiEnum.DangHoatDong
            );
        }).ToList();

        // 9. Top KTV xử lý nhiều phiếu hoàn thành nhất
        var topKtv = theoKtv
            .Where(k => k.SoPhieuHoanThanh > 0)
            .OrderByDescending(k => k.SoPhieuHoanThanh)
            .Take(5)
            .ToList();

        // 10. Top 5 thiết bị tốn chi phí bảo trì & sửa chữa nhất
        var topChiPhi = thietBis.Select(tb =>
        {
            var phieuTb = phieus.Where(p => (p.BaoCaoSuCo?.MaThietBi ?? p.KeHoachBaoTri?.MaThietBi) == tb.MaThietBi).ToList();
            decimal cp = phieuTb.Where(p => p.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy).Sum(p => p.TongChiPhi);
            int soVatTu = phieuTb.SelectMany(p => p.ChiTietXuLys).Count(c => c.ThanhTien.HasValue);

            return new ThietBiTonChiPhiItem(
                tb.MaThietBi,
                tb.TenThietBi,
                tb.SoSerial,
                tb.LoaiThietBi?.TenLoaiThietBi,
                tb.KhuVuc?.TenKhuVuc,
                phieuTb.Count,
                soVatTu,
                cp
            );
        })
        .Where(x => x.TongChiPhi > 0)
        .OrderByDescending(x => x.TongChiPhi)
        .Take(5)
        .ToList();

        // 11. Thống kê chi phí theo 12 tháng của năm được chọn
        var allChiTietTrongNam = await _context.ChiTietXuLys
            .AsNoTracking()
            .Include(c => c.PhieuXuLy)
            .Where(c => c.NgayCapNhat.Year == namChon
                     && c.ThanhTien != null
                     && c.PhieuXuLy != null
                     && c.PhieuXuLy.TrangThai != TrangThaiPhieuXuLyEnum.DaHuy)
            .ToListAsync();

        var chiPhi12Thang = Enumerable.Range(1, 12).Select(m =>
        {
            var ctThang = allChiTietTrongNam.Where(c => c.NgayCapNhat.Month == m).ToList();
            var soPhieu = ctThang.Select(c => c.MaPhieuXuLy).Distinct().Count();
            var tongTien = ctThang.Sum(c => c.ThanhTien ?? 0m);

            return new ChiPhiTheoThangItem(m, namChon, soPhieu, tongTien);
        }).ToList();

        return new ThongKeIndexViewModel
        {
            BoLoc = boLoc,
            TongThietBi = tongThietBi,
            TongSuCo = tongSuCo,
            TongKeHoach = tongKeHoach,
            TongPhieuXuLy = tongPhieu,
            TongPhieuHoanThanh = tongPhieuHoanThanh,
            TongChiPhi = tongChiPhi,
            ThoiGianXuLyTbGio = tgXuLyTbGio,

            TheoLoaiThietBi = theoLoai,
            TheoKhuVuc = theoKhuVuc,
            TheoMucDoSuCo = theoMucDo,
            TheoKyThuatVien = theoKtv,

            TopThietBiNhieuSuCo = topNhieuSuCo,
            TopKtvHoanThanhNhieuNhat = topKtv,
            TopThietBiTonChiPhi = topChiPhi,
            ChiPhi12Thang = chiPhi12Thang
        };
    }
}
