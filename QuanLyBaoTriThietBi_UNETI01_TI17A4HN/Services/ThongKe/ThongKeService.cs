// Họ tên: Nguyễn Khắc Hiếu
// MSSV: 23103100225
// Nội dung: Module 5 - Service Báo cáo thống kê (SST 45: LINQ GroupBy đa chiều & STT 61: Xuất Excel chuẩn hóa)
#nullable enable
using ClosedXML.Excel;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class ThongKeService : IThongKeService
{
    private readonly IThongKeRepository _repo;

    public ThongKeService(IThongKeRepository repo) => _repo = repo;

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

        // 2. Tải toàn bộ dữ liệu cần thiết phục vụ thống kê qua Repository
        var thietBis = await _repo.LayDanhSachThietBiAsync();
        var loaiThietBis = await _repo.LayDanhSachLoaiThietBiAsync();
        var khuVucs = await _repo.LayDanhSachKhuVucAsync();
        var baoCaos = await _repo.LayBaoCaoSuCoTheoKhoangNgayAsync(startDate, endDate);
        var keHoachs = await _repo.LayKeHoachBaoTriTheoKhoangNgayAsync(startDate, endDate);
        var phieus = await _repo.LayPhieuXuLyTheoKhoangNgayAsync(startDate, endDate);
        var ktvs = await _repo.LayDanhSachKyThuatVienAsync();

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
        var allChiTietTrongNam = await _repo.LayChiTietXuLyTrongNamAsync(namChon);

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

    /// <summary>
    /// STT 61: Xuất báo cáo thống kê đa chiều ra tệp Excel (.xlsx) chuẩn hóa
    /// Tích hợp đầy đủ các sheet: Chi phí 12 tháng, Theo Loại thiết bị, Theo Khu vực, Hiệu suất KTV, Top thiết bị sự cố & chi phí
    /// </summary>
    public async Task<byte[]> XuatExcelAsync(ThongKeBoLoc boLoc, string? nguoiXuat = null)
    {
        var vm = await LayThongKeAsync(boLoc);
        var namText = boLoc.CoBoLocKhoangNgay
            ? $"Từ {boLoc.TuNgay:dd/MM/yyyy} đến {boLoc.DenNgay:dd/MM/yyyy}"
            : $"Năm {boLoc.Nam ?? DateTime.Today.Year}";

        using var workbook = new XLWorkbook();

        // Sheet 1: Chi phí 12 tháng
        var colChiPhiThang = new List<ExcelColumn<ChiPhiTheoThangItem>>
        {
            new("Tháng", c => $"Tháng {c.Thang}/{c.Nam}", alignment: XLAlignmentHorizontalValues.Center, width: 16),
            new("Số phiếu xử lý phát sinh", c => c.SoPhieuPhatSinh, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 26),
            new("Tổng chi phí bảo trì & sửa chữa", c => c.TongChiPhi, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 32)
        };
        ExcelExportHelper.AddWorksheet(
            workbook,
            "ChiPhi_12Thang",
            $"BÁO CÁO TỔNG HỢP CHI PHÍ BẢO TRÌ THEO THÁNG ({namText})",
            vm.ChiPhi12Thang,
            colChiPhiThang,
            nguoiXuat,
            coCotStt: true
        );

        // Sheet 2: Theo Loại thiết bị
        var colLoai = new List<ExcelColumn<ThongKeNhomItem>>
        {
            new("Tên loại thiết bị", x => x.TenNhom, width: 28),
            new("Số thiết bị", x => x.SoThietBi, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 16),
            new("Số sự cố phát sinh", x => x.SoSuCo, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Số kế hoạch bảo trì", x => x.SoKeHoach, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Số phiếu xử lý", x => x.SoPhieuXuLy, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Tổng chi phí", x => x.TongChiPhi, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 24)
        };
        ExcelExportHelper.AddWorksheet(
            workbook,
            "Theo_LoaiThietBi",
            $"THỐNG KÊ HOẠT ĐỘNG BẢO TRÌ THEO LOẠI THIẾT BỊ ({namText})",
            vm.TheoLoaiThietBi,
            colLoai,
            nguoiXuat,
            coCotStt: true
        );

        // Sheet 3: Theo Khu vực
        var colKhuVuc = new List<ExcelColumn<ThongKeNhomItem>>
        {
            new("Tên khu vực / Phòng ban", x => x.TenNhom, width: 28),
            new("Số thiết bị", x => x.SoThietBi, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 16),
            new("Số sự cố phát sinh", x => x.SoSuCo, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Số kế hoạch bảo trì", x => x.SoKeHoach, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Số phiếu xử lý", x => x.SoPhieuXuLy, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Tổng chi phí", x => x.TongChiPhi, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 24)
        };
        ExcelExportHelper.AddWorksheet(
            workbook,
            "Theo_KhuVuc",
            $"THỐNG KÊ HOẠT ĐỘNG BẢO TRÌ THEO KHU VỰC VẬN HÀNH ({namText})",
            vm.TheoKhuVuc,
            colKhuVuc,
            nguoiXuat,
            coCotStt: true
        );

        // Sheet 4: Hiệu suất Kỹ thuật viên
        var colKtv = new List<ExcelColumn<ThongKeKtvItem>>
        {
            new("Kỹ thuật viên", x => x.TenKyThuatVien, width: 26),
            new("Chuyên môn", x => x.ChuyenMon ?? "—", width: 22),
            new("Số điện thoại", x => x.SoDienThoai ?? "—", alignment: XLAlignmentHorizontalValues.Center, width: 16),
            new("Tổng phiếu phụ trách", x => x.TongPhieuPhuTrach, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 22),
            new("Phiếu hoàn thành", x => x.SoPhieuHoanThanh, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Phiếu đang xử lý", x => x.SoPhieuDangXuLy, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("TG xử lý TB (giờ)", x => x.ThoiGianXuLyTrungBinhGio, numberFormat: "#,##0.0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Tổng chi phí vật tư", x => x.TongChiPhiVatTu, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 24)
        };
        ExcelExportHelper.AddWorksheet(
            workbook,
            "HieuSuat_KTV",
            $"BÁO CÁO ĐÁNH GIÁ HIỆU SUẤT LÀM VIỆC KỸ THUẬT VIÊN ({namText})",
            vm.TheoKyThuatVien,
            colKtv,
            nguoiXuat,
            coCotStt: true
        );

        // Sheet 5: Top thiết bị nhiều sự cố
        var colTopSuCo = new List<ExcelColumn<ThietBiNhieuSuCoItem>>
        {
            new("Tên thiết bị", x => x.TenThietBi, width: 28),
            new("Số Serial", x => x.SoSerial, alignment: XLAlignmentHorizontalValues.Center, width: 18),
            new("Loại thiết bị", x => x.TenLoaiThietBi ?? "—", width: 22),
            new("Khu vực", x => x.TenKhuVuc ?? "—", width: 22),
            new("Tình trạng máy", x => UiHelper.Badge(x.TinhTrang).Text, alignment: XLAlignmentHorizontalValues.Center, width: 18),
            new("Số sự cố gặp phải", x => x.SoSuCo, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Số phiếu bảo trì", x => x.SoPhieuBaoTri, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Tổng chi phí xử lý", x => x.TongChiPhi, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 24)
        };
        ExcelExportHelper.AddWorksheet(
            workbook,
            "Top_ThietBiSuCo",
            $"DANH SÁCH THIẾT BỊ PHÁT SINH NHIỀU SỰ CỐ NHẤT ({namText})",
            vm.TopThietBiNhieuSuCo,
            colTopSuCo,
            nguoiXuat,
            coCotStt: true
        );

        // Sheet 6: Top thiết bị tốn chi phí nhất
        var colTopChiPhi = new List<ExcelColumn<ThietBiTonChiPhiItem>>
        {
            new("Tên thiết bị", x => x.TenThietBi, width: 28),
            new("Số Serial", x => x.SoSerial, alignment: XLAlignmentHorizontalValues.Center, width: 18),
            new("Loại thiết bị", x => x.TenLoaiThietBi ?? "—", width: 22),
            new("Khu vực", x => x.TenKhuVuc ?? "—", width: 22),
            new("Số phiếu xử lý", x => x.SoPhieuXuLy, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 18),
            new("Số lần dùng vật tư", x => x.SoLanDungVatTu, numberFormat: "#,##0", alignment: XLAlignmentHorizontalValues.Right, width: 20),
            new("Tổng chi phí tích lũy", x => x.TongChiPhi, numberFormat: "#,##0 ₫", alignment: XLAlignmentHorizontalValues.Right, width: 24)
        };
        ExcelExportHelper.AddWorksheet(
            workbook,
            "Top_ThietBiChiPhi",
            $"DANH SÁCH THIẾT BỊ TỐN KÉM CHI PHÍ BẢO TRÌ SỬA CHỮA NHẤT ({namText})",
            vm.TopThietBiTonChiPhi,
            colTopChiPhi,
            nguoiXuat,
            coCotStt: true
        );

        return ExcelExportHelper.SaveToBytes(workbook);
    }
}
