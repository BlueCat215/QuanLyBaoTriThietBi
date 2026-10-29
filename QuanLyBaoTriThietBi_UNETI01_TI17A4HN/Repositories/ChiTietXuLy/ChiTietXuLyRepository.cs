// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Repository Chi tiết xử lý (LINQ: danh sách phiếu, chi tiết theo phiếu, tổng chi phí = SUM(ThanhTien), hỗ trợ phân quyền và bắt đầu xử lý)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
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
            .Include(p => p.ChiTietXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == maPhieuXuLy);

    public async Task<LichSuThietBiViewModel?> LayLichSuThietBiAsync(int maThietBi)
    {
        var tb = await _context.ThietBis
            .AsNoTracking()
            .Include(t => t.LoaiThietBi)
            .Include(t => t.KhuVuc)
            .FirstOrDefaultAsync(t => t.MaThietBi == maThietBi);

        if (tb is null)
            return null;

        var thongTinTb = new ThietBiThongTinLichSu(
            tb.MaThietBi,
            tb.TenThietBi,
            tb.SoSerial,
            tb.LoaiThietBi?.TenLoaiThietBi,
            tb.KhuVuc?.TenKhuVuc,
            tb.TinhTrang,
            tb.NgayDuaVaoSuDung,
            tb.ChuKyBaoTriThang,
            tb.NgayBaoTriGanNhat,
            tb.NgayBaoTriTiepTheo,
            tb.MoTa
        );

        // 1. Kế hoạch bảo trì của thiết bị
        var keHoachs = await _context.KeHoachBaoTris
            .AsNoTracking()
            .Where(k => k.MaThietBi == maThietBi)
            .Include(k => k.PhieuXuLys)
                .ThenInclude(p => p.PhanCongXuLys)
                    .ThenInclude(pc => pc.KyThuatVien)
            .Include(k => k.PhieuXuLys)
                .ThenInclude(p => p.ChiTietXuLys)
            .OrderByDescending(k => k.NgayDuKien)
            .ThenByDescending(k => k.MaKeHoach)
            .ToListAsync();

        // 2. Báo cáo sự cố của thiết bị
        var suCos = await _context.BaoCaoSuCos
            .AsNoTracking()
            .Where(s => s.MaThietBi == maThietBi)
            .Include(s => s.TaiKhoanBaoCao)
            .Include(s => s.PhieuXuLys)
                .ThenInclude(p => p.PhanCongXuLys)
                    .ThenInclude(pc => pc.KyThuatVien)
            .Include(s => s.PhieuXuLys)
                .ThenInclude(p => p.ChiTietXuLys)
            .OrderByDescending(s => s.NgayBaoCao)
            .ThenByDescending(s => s.MaBaoCao)
            .ToListAsync();

        // 3. Toàn bộ phiếu xử lý liên quan đến thiết bị này (qua KeHoachBaoTri hoặc BaoCaoSuCo)
        var phieus = await _context.PhieuXuLys
            .AsNoTracking()
            .Where(p => (p.MaKeHoach != null && p.KeHoachBaoTri!.MaThietBi == maThietBi)
                     || (p.MaBaoCao != null && p.BaoCaoSuCo!.MaThietBi == maThietBi))
            .Include(p => p.KeHoachBaoTri)
            .Include(p => p.BaoCaoSuCo)
            .Include(p => p.PhanCongXuLys)
                .ThenInclude(pc => pc.KyThuatVien)
            .Include(p => p.ChiTietXuLys)
            .OrderByDescending(p => p.NgayTao)
            .ThenByDescending(p => p.MaPhieuXuLy)
            .ToListAsync();

        var dsKeHoach = keHoachs.Select(k =>
        {
            var phieu = k.PhieuXuLys.OrderByDescending(p => p.NgayTao).FirstOrDefault();
            var ktv = phieu?.PhanCongXuLys
                .Where(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                .Select(pc => pc.KyThuatVien?.HoTen)
                .FirstOrDefault()
                ?? phieu?.PhanCongXuLys.OrderByDescending(pc => pc.NgayPhanCong).Select(pc => pc.KyThuatVien?.HoTen).FirstOrDefault();

            var chiPhi = phieu?.ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m) ?? 0m;

            return new LichSuKeHoachItem(
                k.MaKeHoach,
                k.NgayDuKien,
                k.NoiDungBaoTri,
                k.MucDoUuTien,
                k.TrangThai,
                k.GhiChu,
                phieu?.MaPhieuXuLy,
                phieu?.TrangThai,
                ktv,
                phieu?.NgayHoanThanh,
                chiPhi
            );
        }).ToList();

        var dsSuCo = suCos.Select(s =>
        {
            var phieu = s.PhieuXuLys.OrderByDescending(p => p.NgayTao).FirstOrDefault();
            var ktv = phieu?.PhanCongXuLys
                .Where(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                .Select(pc => pc.KyThuatVien?.HoTen)
                .FirstOrDefault()
                ?? phieu?.PhanCongXuLys.OrderByDescending(pc => pc.NgayPhanCong).Select(pc => pc.KyThuatVien?.HoTen).FirstOrDefault();

            var chiPhi = phieu?.ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m) ?? 0m;
            var nguoiBao = s.TaiKhoanBaoCao != null
                ? (!string.IsNullOrWhiteSpace(s.TaiKhoanBaoCao.HoTen) ? s.TaiKhoanBaoCao.HoTen : s.TaiKhoanBaoCao.TenDangNhap)
                : "—";

            return new LichSuSuCoItem(
                s.MaBaoCao,
                s.NgayBaoCao,
                nguoiBao,
                s.MoTaSuCo,
                s.MucDoSuCo,
                s.TrangThai,
                s.GhiChu,
                phieu?.MaPhieuXuLy,
                phieu?.TrangThai,
                ktv,
                phieu?.NgayHoanThanh,
                chiPhi
            );
        }).ToList();

        var dsPhieu = phieus.Select(p =>
        {
            var nguonText = p.LoaiXuLy == LoaiXuLyEnum.XuLySuCo
                ? (p.MaBaoCao.HasValue ? ("Sự cố " + UiHelper.Ma("SC", p.MaBaoCao.Value)) : "Sự cố")
                : (p.MaKeHoach.HasValue ? ("Kế hoạch " + UiHelper.Ma("KH", p.MaKeHoach.Value)) : "Kế hoạch");

            var dsKtv = p.PhanCongXuLys
                .OrderByDescending(pc => pc.NgayPhanCong)
                .Select(pc => new LichSuKtvPhanCong(
                    pc.MaPhanCong,
                    pc.MaKyThuatVien,
                    pc.KyThuatVien?.HoTen ?? "—",
                    pc.KyThuatVien?.SoDienThoai,
                    pc.KyThuatVien?.Email,
                    pc.KyThuatVien?.ChuyenMon,
                    pc.NgayPhanCong,
                    pc.NgayKetThucPhanCong,
                    pc.NoiDungPhanCong,
                    pc.TrangThai,
                    pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc
                )).ToList();

            var dsCt = p.ChiTietXuLys
                .OrderBy(c => c.NgayCapNhat)
                .Select(c => new ChiTietXuLyRow(
                    c.MaChiTiet,
                    c.NgayCapNhat,
                    c.NoiDungThucHien,
                    c.VatTuLinhKien,
                    c.SoLuong,
                    c.DonGia,
                    c.ThanhTien,
                    c.GhiChu
                )).ToList();

            var tongChiPhiPhieu = p.ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m);

            return new LichSuPhieuXuLyItem(
                p.MaPhieuXuLy,
                p.LoaiXuLy,
                p.MaBaoCao,
                p.MaKeHoach,
                nguonText,
                p.NgayTao,
                p.NgayBatDau,
                p.NgayHoanThanh,
                p.TrangThai,
                p.KetQuaXuLy,
                tongChiPhiPhieu,
                dsKtv,
                dsCt
            );
        }).ToList();

        // 4. Tạo chuỗi sự kiện Timeline toàn diện
        var events = new List<TimelineEventItem>();

        // Sự kiện: Bắt đầu đưa vào sử dụng
        events.Add(new TimelineEventItem(
            tb.NgayDuaVaoSuDung,
            "Đưa vào sử dụng",
            "is-maintenance",
            "Thiết bị " + tb.TenThietBi + " bắt đầu đưa vào sử dụng",
            "Số serial: " + tb.SoSerial + ". Chu kỳ bảo trì: " + tb.ChuKyBaoTriThang + " tháng. Khu vực: " + (tb.KhuVuc?.TenKhuVuc ?? "Chưa xác định") + ".",
            null,
            null,
            null,
            "Bắt đầu",
            "badge-primary"
        ));

        // Sự kiện: Các kế hoạch bảo trì
        foreach (var kh in keHoachs)
        {
            events.Add(new TimelineEventItem(
                kh.NgayDuKien,
                "Kế hoạch bảo trì",
                "is-maintenance",
                "Lập kế hoạch bảo trì " + UiHelper.Ma("KH", kh.MaKeHoach),
                kh.NoiDungBaoTri + (string.IsNullOrWhiteSpace(kh.GhiChu) ? "" : " (Ghi chú: " + kh.GhiChu + ")"),
                null,
                null,
                "/KeHoachBaoTri/Details/" + kh.MaKeHoach,
                UiHelper.Badge(kh.TrangThai).Text,
                UiHelper.Badge(kh.TrangThai).Css
            ));
        }

        // Sự kiện: Các báo cáo sự cố
        foreach (var sc in suCos)
        {
            var tenNguoiBao = sc.TaiKhoanBaoCao != null
                ? (!string.IsNullOrWhiteSpace(sc.TaiKhoanBaoCao.HoTen) ? sc.TaiKhoanBaoCao.HoTen : sc.TaiKhoanBaoCao.TenDangNhap)
                : "Hệ thống";

            events.Add(new TimelineEventItem(
                sc.NgayBaoCao,
                "Sự cố phát sinh",
                "is-incident",
                "Báo cáo sự cố " + UiHelper.Ma("SC", sc.MaBaoCao) + ": " + sc.MoTaSuCo,
                "Mức độ: " + UiHelper.Badge(sc.MucDoSuCo).Text + ". Người báo: " + tenNguoiBao + "." + (string.IsNullOrWhiteSpace(sc.GhiChu) ? "" : " (Ghi chú: " + sc.GhiChu + ")"),
                tenNguoiBao,
                null,
                "/BaoCaoSuCo/Details/" + sc.MaBaoCao,
                UiHelper.Badge(sc.TrangThai).Text,
                UiHelper.Badge(sc.TrangThai).Css
            ));
        }

        // Sự kiện: Các phiếu xử lý, phân công KTV, các bước xử lý và hoàn thành
        foreach (var p in phieus)
        {
            var maPhieuFormatted = UiHelper.Ma("PX", p.MaPhieuXuLy);
            var urlPhieu = "/ChiTietXuLy/TheoPhieu?maPhieu=" + p.MaPhieuXuLy;

            // Tạo phiếu
            events.Add(new TimelineEventItem(
                p.NgayTao,
                "Tạo phiếu xử lý",
                p.LoaiXuLy == LoaiXuLyEnum.XuLySuCo ? "is-incident" : "is-maintenance",
                "Khởi tạo phiếu xử lý " + maPhieuFormatted,
                "Nguồn: " + (p.LoaiXuLy == LoaiXuLyEnum.XuLySuCo ? (p.MaBaoCao.HasValue ? ("Sự cố " + UiHelper.Ma("SC", p.MaBaoCao.Value)) : "Sự cố") : (p.MaKeHoach.HasValue ? ("Kế hoạch " + UiHelper.Ma("KH", p.MaKeHoach.Value)) : "Kế hoạch")) + ".",
                null,
                null,
                urlPhieu,
                UiHelper.Badge(p.TrangThai).Text,
                UiHelper.Badge(p.TrangThai).Css
            ));

            // Lịch sử phân công KTV (bảo toàn cả KTV cũ và KTV mới)
            foreach (var pc in p.PhanCongXuLys)
            {
                var tenKtv = pc.KyThuatVien?.HoTen ?? "KTV";
                if (pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                {
                    events.Add(new TimelineEventItem(
                        pc.NgayPhanCong,
                        "Phân công KTV",
                        "is-maintenance",
                        "Phân công KTV " + tenKtv + " phụ trách phiếu " + maPhieuFormatted,
                        "Nội dung: " + (pc.NoiDungPhanCong ?? "Thực hiện xử lý bảo trì/sự cố") + ". SĐT: " + (pc.KyThuatVien?.SoDienThoai ?? "—") + ".",
                        tenKtv,
                        null,
                        urlPhieu,
                        "Đang hiệu lực",
                        "badge-success"
                    ));
                }
                else
                {
                    // Lịch sử đổi KTV / Bàn giao
                    events.Add(new TimelineEventItem(
                        pc.NgayKetThucPhanCong ?? pc.NgayPhanCong,
                        "Bàn giao / Đổi KTV",
                        "is-reassign",
                        "KTV " + tenKtv + " kết thúc phân công trên phiếu " + maPhieuFormatted,
                        "Chuyển giao hoặc thay đổi KTV phụ trách theo quy trình bảo toàn lịch sử.",
                        tenKtv,
                        null,
                        urlPhieu,
                        "Đã bàn giao",
                        "badge-secondary"
                    ));
                }
            }

            // Bắt đầu xử lý
            if (p.NgayBatDau.HasValue)
            {
                events.Add(new TimelineEventItem(
                    p.NgayBatDau.Value,
                    "Bắt đầu xử lý",
                    "is-maintenance",
                    "KTV bắt đầu thực hiện phiếu " + maPhieuFormatted,
                    "Thiết bị và phiếu được ghi nhận chuyển sang trạng thái Đang xử lý.",
                    null,
                    null,
                    urlPhieu,
                    "Đang xử lý",
                    "badge-primary"
                ));
            }

            // Chi tiết xử lý và vật tư linh kiện thay thế
            foreach (var ct in p.ChiTietXuLys)
            {
                var thongTinVatTu = string.IsNullOrWhiteSpace(ct.VatTuLinhKien)
                    ? "Không sử dụng vật tư."
                    : "Vật tư: " + ct.VatTuLinhKien + " (SL: " + (ct.SoLuong ?? 0) + ", Đơn giá: " + UiHelper.Tien(ct.DonGia) + ", Thành tiền: " + UiHelper.Tien(ct.ThanhTien) + ").";

                events.Add(new TimelineEventItem(
                    ct.NgayCapNhat,
                    "Xử lý & Vật tư",
                    "is-maintenance",
                    "Phiếu " + maPhieuFormatted + ": " + ct.NoiDungThucHien,
                    thongTinVatTu + (string.IsNullOrWhiteSpace(ct.GhiChu) ? "" : " (Ghi chú: " + ct.GhiChu + ")"),
                    null,
                    ct.ThanhTien,
                    urlPhieu,
                    ct.ThanhTien > 0 ? UiHelper.Tien(ct.ThanhTien) : "Chi tiết",
                    "badge-info"
                ));
            }

            // Nghiệm thu hoàn thành
            if (p.NgayHoanThanh.HasValue)
            {
                var tongChiPhiPhieu = p.ChiTietXuLys.Sum(c => c.ThanhTien ?? 0m);
                events.Add(new TimelineEventItem(
                    p.NgayHoanThanh.Value,
                    "Hoàn thành xử lý",
                    "is-done",
                    "Nghiệm thu hoàn thành phiếu " + maPhieuFormatted,
                    "Kết quả: " + (p.KetQuaXuLy ?? "Đạt yêu cầu") + ". Tổng chi phí phiếu: " + UiHelper.Tien(tongChiPhiPhieu) + ".",
                    null,
                    tongChiPhiPhieu,
                    urlPhieu,
                    "Hoàn thành",
                    "badge-success"
                ));
            }
        }

        // Sắp xếp timeline giảm dần theo thời gian (sự kiện mới nhất lên trước)
        var sortedEvents = events.OrderByDescending(e => e.Ngay).ToList();

        return new LichSuThietBiViewModel
        {
            ThietBi = thongTinTb,
            TongSoKeHoach = dsKeHoach.Count,
            TongSoSuCo = dsSuCo.Count,
            TongSoPhieuXuLy = dsPhieu.Count,
            SoPhieuHoanThanh = dsPhieu.Count(p => p.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh),
            TongChiPhiLuyKe = dsPhieu.Sum(p => p.TongChiPhi),
            KeHoachBaoTris = dsKeHoach,
            BaoCaoSuCos = dsSuCo,
            PhieuXuLys = dsPhieu,
            TimelineEvents = sortedEvents,
        };
    }

}
