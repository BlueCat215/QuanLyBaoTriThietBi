// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Service triển khai nghiệp vụ Phiếu xử lý, Phân công, Đổi KTV giữ lịch sử và quy tắc sự cố
#nullable enable
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Data;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class PhieuXuLyService : IPhieuXuLyService
{
    private readonly IPhieuXuLyRepository _phieuRepo;
    private readonly AppDbContext _context;

    public PhieuXuLyService(IPhieuXuLyRepository phieuRepo, AppDbContext context)
    {
        _phieuRepo = phieuRepo;
        _context = context;
    }

    public async Task<PhieuXuLyIndexViewModel> LayDanhSachAsync(PhieuXuLyBoLoc boLoc)
    {
        var pagedResult = await _phieuRepo.LayPhanTrangAsync(boLoc);
        var ktvList = await _phieuRepo.LayKtvDangHoatDongChoChonAsync();

        return new PhieuXuLyIndexViewModel
        {
            PagedItems = pagedResult,
            BoLoc = boLoc,
            KtvOptions = ktvList.Select(k => new SelectListItem(k.Ten, k.Ma.ToString())).ToList()
        };
    }

    public async Task<PhieuXuLyIndexViewModel> LayViecCuaKtvAsync(int maTaiKhoan, PhieuXuLyBoLoc boLoc)
    {
        var ktv = await _phieuRepo.LayKtvTheoTaiKhoanAsync(maTaiKhoan);
        if (ktv == null)
        {
            return new PhieuXuLyIndexViewModel
            {
                PagedItems = new PagedResult<PhieuXuLyDanhSachRow> { Items = new(), TotalItems = 0, Page = 1, PageSize = boLoc.PageSize },
                BoLoc = boLoc
            };
        }

        var pagedResult = await _phieuRepo.LayPhanTrangTheoKtvAsync(ktv.MaKyThuatVien, boLoc);
        return new PhieuXuLyIndexViewModel
        {
            PagedItems = pagedResult,
            BoLoc = boLoc
        };
    }

    public async Task<PhieuXuLyDetailsViewModel?> LayChiTietAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap, bool laAdmin)
    {
        var phieu = await _phieuRepo.LayChiTietAsync(maPhieuXuLy);
        if (phieu == null) return null;

        var lichSu = await _phieuRepo.LayLichSuPhanCongAsync(maPhieuXuLy);
        var ktvHienTai = lichSu.FirstOrDefault(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc);

        KyThuatVien? ktvDangNhap = null;
        if (maTaiKhoanDangNhap.HasValue)
        {
            ktvDangNhap = await _phieuRepo.LayKtvTheoTaiKhoanAsync(maTaiKhoanDangNhap.Value);
        }

        bool laKtvPhuTrach = ktvDangNhap != null && ktvHienTai != null && ktvHienTai.MaKyThuatVien == ktvDangNhap.MaKyThuatVien;

        var ktvActiveList = await _phieuRepo.LayKtvDangHoatDongChoChonAsync();
        var ktvOptions = ktvActiveList
            .Where(k => ktvHienTai == null || k.Ma != ktvHienTai.MaKyThuatVien)
            .Select(k => new SelectListItem(k.Ten, k.Ma.ToString()))
            .ToList();

        var vm = new PhieuXuLyDetailsViewModel
        {
            MaPhieuXuLy = phieu.MaPhieuXuLy,
            LoaiXuLy = phieu.LoaiXuLy,
            TrangThai = phieu.TrangThai,
            NgayTao = phieu.NgayTao,
            NgayBatDau = phieu.NgayBatDau,
            NgayHoanThanh = phieu.NgayHoanThanh,
            KetQuaXuLy = phieu.KetQuaXuLy,
            TongChiPhi = phieu.TongChiPhi,
            KtvHienTai = ktvHienTai,
            LichSuPhanCong = lichSu,
            LaAdmin = laAdmin,
            LaKtvPhuTrach = laKtvPhuTrach,
            KtvOptions = ktvOptions
        };

        if (phieu.LoaiXuLy == LoaiXuLyEnum.XuLySuCo && phieu.BaoCaoSuCo != null)
        {
            var bc = phieu.BaoCaoSuCo;
            vm.MaBaoCao = bc.MaBaoCao;
            vm.MoTaSuCo = bc.MoTaSuCo;
            vm.MucDoSuCo = bc.MucDoSuCo;
            vm.NguoiBaoCao = bc.TaiKhoanBaoCao?.HoTen;

            if (bc.ThietBi != null)
            {
                vm.MaThietBi = bc.ThietBi.MaThietBi;
                vm.TenThietBi = bc.ThietBi.TenThietBi;
                vm.SoSerial = bc.ThietBi.SoSerial;
                vm.TenLoaiThietBi = bc.ThietBi.LoaiThietBi?.TenLoaiThietBi;
                vm.TenKhuVuc = bc.ThietBi.KhuVuc?.TenKhuVuc;
                vm.TinhTrangThietBi = bc.ThietBi.TinhTrang;
            }
        }
        else if (phieu.LoaiXuLy == LoaiXuLyEnum.BaoTriDinhKy && phieu.KeHoachBaoTri != null)
        {
            var kh = phieu.KeHoachBaoTri;
            vm.MaKeHoach = kh.MaKeHoach;
            vm.NoiDungBaoTri = kh.NoiDungBaoTri;
            vm.MucDoUuTien = kh.MucDoUuTien;
            vm.NgayDuKienBaoTri = kh.NgayDuKien;

            if (kh.ThietBi != null)
            {
                vm.MaThietBi = kh.ThietBi.MaThietBi;
                vm.TenThietBi = kh.ThietBi.TenThietBi;
                vm.SoSerial = kh.ThietBi.SoSerial;
                vm.TenLoaiThietBi = kh.ThietBi.LoaiThietBi?.TenLoaiThietBi;
                vm.TenKhuVuc = kh.ThietBi.KhuVuc?.TenKhuVuc;
                vm.TinhTrangThietBi = kh.ThietBi.TinhTrang;
            }
        }

        return vm;
    }

    public async Task<TaoPhieuXuLyViewModel> ChuanBiFormTaoPhieuAsync(int? maBaoCao, int? maKeHoach)
    {
        var vm = new TaoPhieuXuLyViewModel
        {
            MaBaoCao = maBaoCao,
            MaKeHoach = maKeHoach,
            LoaiXuLy = maKeHoach.HasValue ? LoaiXuLyEnum.BaoTriDinhKy : LoaiXuLyEnum.XuLySuCo
        };

        var bcList = await _phieuRepo.LayBaoCaoChoChonAsync();
        vm.BaoCaoOptions = bcList.Select(b => new SelectListItem(b.MoTa, b.Ma.ToString())).ToList();

        var khList = await _phieuRepo.LayKeHoachChoChonAsync();
        vm.KeHoachOptions = khList.Select(k => new SelectListItem(k.MoTa, k.Ma.ToString())).ToList();

        var ktvList = await _phieuRepo.LayKtvDangHoatDongChoChonAsync();
        vm.KtvOptions = ktvList.Select(k => new SelectListItem(k.Ten, k.Ma.ToString())).ToList();

        return vm;
    }

    /// STT 35 & STT 37: Tạo phiếu xử lý và áp dụng quy tắc sự cố ảnh hưởng trạng thái thiết bị
    public async Task<ServiceResult<int>> TaoPhieuXuLyAsync(TaoPhieuXuLyViewModel model)
    {
        if (model.LoaiXuLy == LoaiXuLyEnum.XuLySuCo)
        {
            if (!model.MaBaoCao.HasValue)
                return ServiceResult<int>.Fail("Vui lòng chọn báo cáo sự cố nguồn.");

            var bc = await _context.BaoCaoSuCos
                .Include(b => b.ThietBi)
                .FirstOrDefaultAsync(b => b.MaBaoCao == model.MaBaoCao.Value);

            if (bc == null)
                return ServiceResult<int>.Fail("Báo cáo sự cố không tồn tại.");

            if (bc.TrangThai == TrangThaiBaoCaoEnum.DaHuy)
                return ServiceResult<int>.Fail("Báo cáo sự cố đã bị hủy, không thể lập phiếu xử lý.");

            if (await _phieuRepo.DaCoPhieuDangHoatDongChoBaoCaoAsync(bc.MaBaoCao))
                return ServiceResult<int>.Fail($"Báo cáo {UiHelper.Ma("SC", bc.MaBaoCao)} đã có phiếu xử lý đang hoạt động.");

            var phieu = new PhieuXuLy
            {
                LoaiXuLy = LoaiXuLyEnum.XuLySuCo,
                MaBaoCao = bc.MaBaoCao,
                MaKeHoach = null,
                NgayTao = DateTime.Now,
                TrangThai = model.MaKyThuatVien.HasValue ? TrangThaiPhieuXuLyEnum.DaPhanCong : TrangThaiPhieuXuLyEnum.ChoPhanCong
            };

            // STT 37: Quy tắc nghiệp vụ - Sự cố mức Cao hoặc Khẩn cấp có thể chuyển thiết bị sang Tạm ngừng do sự cố
            bool laSuCoNghiemTrong = bc.MucDoSuCo == MucDoSuCoEnum.Cao || bc.MucDoSuCo == MucDoSuCoEnum.KhanCap;
            if (model.TamNgungThietBi || laSuCoNghiemTrong)
            {
                if (bc.ThietBi != null && bc.ThietBi.TinhTrang == TinhTrangThietBiEnum.DangHoatDong)
                {
                    bc.ThietBi.TinhTrang = TinhTrangThietBiEnum.TamNgungDoSuCo;
                }
            }

            // Cập nhật trạng thái báo cáo sự cố
            if (bc.TrangThai == TrangThaiBaoCaoEnum.MoiBao)
            {
                bc.TrangThai = TrangThaiBaoCaoEnum.DaTiepNhan;
            }

            await _phieuRepo.AddAsync(phieu);
            await _phieuRepo.SaveChangesAsync();

            // Nếu có phân công KTV ngay khi tạo phiếu
            if (model.MaKyThuatVien.HasValue)
            {
                if (!await _phieuRepo.KtvDangHoatDongAsync(model.MaKyThuatVien.Value))
                    return ServiceResult<int>.Fail("Kỹ thuật viên được chọn không tồn tại hoặc đã ngừng hoạt động.");

                var pc = new PhanCongXuLy
                {
                    MaPhieuXuLy = phieu.MaPhieuXuLy,
                    MaKyThuatVien = model.MaKyThuatVien.Value,
                    NgayPhanCong = DateTime.Now,
                    TrangThai = TrangThaiPhanCongEnum.DangHieuLuc,
                    NoiDungPhanCong = model.NoiDungPhanCong ?? "Phân công xử lý sự cố"
                };

                await _phieuRepo.ThemPhanCongAsync(pc);
                await _phieuRepo.SaveChangesAsync();
            }

            return ServiceResult<int>.Ok(phieu.MaPhieuXuLy);
        }
        else // BaoTriDinhKy
        {
            if (!model.MaKeHoach.HasValue)
                return ServiceResult<int>.Fail("Vui lòng chọn kế hoạch bảo trì nguồn.");

            var kh = await _context.KeHoachBaoTris
                .Include(k => k.ThietBi)
                .FirstOrDefaultAsync(k => k.MaKeHoach == model.MaKeHoach.Value);

            if (kh == null)
                return ServiceResult<int>.Fail("Kế hoạch bảo trì không tồn tại.");

            if (kh.TrangThai == TrangThaiKeHoachEnum.DaHuy || kh.TrangThai == TrangThaiKeHoachEnum.HoanThanh)
                return ServiceResult<int>.Fail("Kế hoạch bảo trì đã hoàn thành hoặc đã hủy, không thể lập phiếu xử lý.");

            if (await _phieuRepo.DaCoPhieuDangHoatDongChoKeHoachAsync(kh.MaKeHoach))
                return ServiceResult<int>.Fail($"Kế hoạch {UiHelper.Ma("KH", kh.MaKeHoach)} đã có phiếu xử lý đang hoạt động.");

            var phieu = new PhieuXuLy
            {
                LoaiXuLy = LoaiXuLyEnum.BaoTriDinhKy,
                MaBaoCao = null,
                MaKeHoach = kh.MaKeHoach,
                NgayTao = DateTime.Now,
                TrangThai = model.MaKyThuatVien.HasValue ? TrangThaiPhieuXuLyEnum.DaPhanCong : TrangThaiPhieuXuLyEnum.ChoPhanCong
            };

            if (kh.TrangThai == TrangThaiKeHoachEnum.ChoThucHien)
            {
                kh.TrangThai = model.MaKyThuatVien.HasValue ? TrangThaiKeHoachEnum.DaPhanCong : TrangThaiKeHoachEnum.ChoThucHien;
            }

            await _phieuRepo.AddAsync(phieu);
            await _phieuRepo.SaveChangesAsync();

            if (model.MaKyThuatVien.HasValue)
            {
                if (!await _phieuRepo.KtvDangHoatDongAsync(model.MaKyThuatVien.Value))
                    return ServiceResult<int>.Fail("Kỹ thuật viên được chọn không tồn tại hoặc đã ngừng hoạt động.");

                var pc = new PhanCongXuLy
                {
                    MaPhieuXuLy = phieu.MaPhieuXuLy,
                    MaKyThuatVien = model.MaKyThuatVien.Value,
                    NgayPhanCong = DateTime.Now,
                    TrangThai = TrangThaiPhanCongEnum.DangHieuLuc,
                    NoiDungPhanCong = model.NoiDungPhanCong ?? "Phân công bảo trì định kỳ"
                };

                await _phieuRepo.ThemPhanCongAsync(pc);
                await _phieuRepo.SaveChangesAsync();
            }

            return ServiceResult<int>.Ok(phieu.MaPhieuXuLy);
        }
    }

    /// STT 35: Phân công xử lý lần đầu cho phiếu đang Chờ phân công
    public async Task<ServiceResult> PhanCongXuLyAsync(PhanCongXuLyFormViewModel model)
    {
        var phieu = await _context.PhieuXuLys
            .Include(p => p.KeHoachBaoTri)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == model.MaPhieuXuLy);

        if (phieu == null)
            return ServiceResult.Fail("Phiếu xử lý không tồn tại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh || phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu xử lý đã hoàn thành hoặc đã hủy, không thể phân công.");

        if (!model.MaKyThuatVien.HasValue)
            return ServiceResult.Fail("Vui lòng chọn kỹ thuật viên.");

        if (!await _phieuRepo.KtvDangHoatDongAsync(model.MaKyThuatVien.Value))
            return ServiceResult.Fail("Kỹ thuật viên không tồn tại hoặc đã ngừng hoạt động.");

        // Kiểm tra xem đã có phân công hiệu lực nào chưa (chống 2 phân công hiệu lực cùng 1 phiếu)
        var daCoHieuLuc = await _phieuRepo.LayPhanCongHienTaiAsync(model.MaPhieuXuLy);
        if (daCoHieuLuc != null)
            return ServiceResult.Fail("Phiếu này đã có kỹ thuật viên phụ trách. Nếu muốn đổi KTV, vui lòng dùng chức năng Đổi kỹ thuật viên.");

        var pc = new PhanCongXuLy
        {
            MaPhieuXuLy = phieu.MaPhieuXuLy,
            MaKyThuatVien = model.MaKyThuatVien.Value,
            NgayPhanCong = DateTime.Now,
            TrangThai = TrangThaiPhanCongEnum.DangHieuLuc,
            NoiDungPhanCong = model.NoiDungPhanCong ?? "Phân công xử lý"
        };

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.ChoPhanCong)
        {
            phieu.TrangThai = TrangThaiPhieuXuLyEnum.DaPhanCong;
        }

        if (phieu.KeHoachBaoTri != null && phieu.KeHoachBaoTri.TrangThai == TrangThaiKeHoachEnum.ChoThucHien)
        {
            phieu.KeHoachBaoTri.TrangThai = TrangThaiKeHoachEnum.DaPhanCong;
        }

        await _phieuRepo.ThemPhanCongAsync(pc);
        await _phieuRepo.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    /// STT 36: Đổi kỹ thuật viên (không sửa đè bản ghi cũ, kết thúc phân công cũ, tạo phân công mới để giữ lịch sử)
    public async Task<ServiceResult> DoiKyThuatVienAsync(DoiKyThuatVienFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.LyDoDoi))
            return ServiceResult.Fail("Vui lòng nhập lý do đổi kỹ thuật viên để lưu vết lịch sử.");

        if (!model.MaKyThuatVienMoi.HasValue)
            return ServiceResult.Fail("Vui lòng chọn kỹ thuật viên mới.");

        var phieu = await _context.PhieuXuLys.FindAsync(model.MaPhieuXuLy);
        if (phieu == null)
            return ServiceResult.Fail("Phiếu xử lý không tồn tại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh || phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu xử lý đã hoàn thành hoặc đã hủy, không thể đổi kỹ thuật viên.");

        if (!await _phieuRepo.KtvDangHoatDongAsync(model.MaKyThuatVienMoi.Value))
            return ServiceResult.Fail("Kỹ thuật viên mới không tồn tại hoặc đã ngừng hoạt động.");

        // Tìm phân công đang hiệu lực
        var phanCongCu = await _phieuRepo.LayPhanCongHienTaiAsync(model.MaPhieuXuLy);
        if (phanCongCu != null)
        {
            if (phanCongCu.MaKyThuatVien == model.MaKyThuatVienMoi.Value)
                return ServiceResult.Fail("Kỹ thuật viên này hiện đang phụ trách phiếu rồi, không cần đổi.");

            // 1. Kết thúc phân công cũ: chuyển sang DaKetThuc và gán NgayKetThucPhanCong
            phanCongCu.TrangThai = TrangThaiPhanCongEnum.DaKetThuc;
            phanCongCu.NgayKetThucPhanCong = DateTime.Now;

            // BẮT BUỘC SaveChanges trước để giải phóng ràng buộc duy nhất (Unique Filtered Index: UX_PhanCong_DangHieuLuc)
            await _context.SaveChangesAsync();
        }

        // 2. Tạo phân công mới với trạng thái DangHieuLuc
        var phanCongMoi = new PhanCongXuLy
        {
            MaPhieuXuLy = model.MaPhieuXuLy,
            MaKyThuatVien = model.MaKyThuatVienMoi.Value,
            NgayPhanCong = DateTime.Now,
            TrangThai = TrangThaiPhanCongEnum.DangHieuLuc,
            NoiDungPhanCong = $"[Đổi KTV] {model.LyDoDoi.Trim()}"
        };

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.ChoPhanCong)
        {
            phieu.TrangThai = TrangThaiPhieuXuLyEnum.DaPhanCong;
        }

        await _phieuRepo.ThemPhanCongAsync(phanCongMoi);
        await _phieuRepo.SaveChangesAsync();

        return ServiceResult.Ok();
    }

    /// STT 36: Truy vấn LINQ KTV hiện tại
    public Task<KyThuatVien?> LayKtvHienTaiAsync(int maPhieuXuLy) =>
        _phieuRepo.LayKtvHienTaiAsync(maPhieuXuLy);

    /// STT 39: Bắt đầu xử lý phiếu (chỉ KTV đang được phân công hoặc Admin)
    /// Ghi nhận NgayBatDau, chuyển phiếu sang Đang xử lý, cập nhật trạng thái nguồn và thiết bị
    public async Task<ServiceResult> BatDauXuLyAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap, bool laAdmin)
    {
        var phieu = await _context.PhieuXuLys
            .Include(p => p.BaoCaoSuCo).ThenInclude(b => b!.ThietBi)
            .Include(p => p.KeHoachBaoTri).ThenInclude(k => k!.ThietBi)
            .Include(p => p.PhanCongXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == maPhieuXuLy);

        if (phieu == null)
            return ServiceResult.Fail("Phiếu xử lý không tồn tại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.DangXuLy)
            return ServiceResult.Fail("Phiếu này đã bắt đầu xử lý trước đó rồi.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.ChoNghiemThu)
            return ServiceResult.Fail("Phiếu đang chờ nghiệm thu, không thể bắt đầu xử lý lại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh || phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu đã kết thúc (Hoàn thành / Đã hủy), không thể bắt đầu xử lý.");

        var phanCongHienTai = phieu.PhanCongXuLys
            .FirstOrDefault(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc);

        if (phanCongHienTai == null)
            return ServiceResult.Fail("Phiếu chưa được phân công cho kỹ thuật viên nào.");

        if (!laAdmin)
        {
            if (!maTaiKhoanDangNhap.HasValue)
                return ServiceResult.Fail("Không xác định được danh tính người dùng đăng nhập.");

            var ktv = await _phieuRepo.LayKtvTheoTaiKhoanAsync(maTaiKhoanDangNhap.Value);
            if (ktv == null || ktv.MaKyThuatVien != phanCongHienTai.MaKyThuatVien)
                return ServiceResult.Fail("Chỉ kỹ thuật viên đang được phân công phụ trách phiếu này mới có quyền bắt đầu xử lý.");
        }

        // 1. Cập nhật phiếu xử lý: ghi nhận NgayBatDau và chuyển trạng thái sang DangXuLy
        phieu.NgayBatDau ??= DateTime.Now;
        phieu.TrangThai = TrangThaiPhieuXuLyEnum.DangXuLy;

        // 2. Đồng bộ nguồn sự cố / kế hoạch và tình trạng thiết bị
        if (phieu.LoaiXuLy == LoaiXuLyEnum.XuLySuCo && phieu.BaoCaoSuCo != null)
        {
            if (phieu.BaoCaoSuCo.TrangThai != TrangThaiBaoCaoEnum.DangXuLy)
            {
                phieu.BaoCaoSuCo.TrangThai = TrangThaiBaoCaoEnum.DangXuLy;
            }

            if (phieu.BaoCaoSuCo.ThietBi != null && phieu.BaoCaoSuCo.ThietBi.TinhTrang == TinhTrangThietBiEnum.DangHoatDong)
            {
                phieu.BaoCaoSuCo.ThietBi.TinhTrang = TinhTrangThietBiEnum.TamNgungDoSuCo;
            }
        }
        else if (phieu.LoaiXuLy == LoaiXuLyEnum.BaoTriDinhKy && phieu.KeHoachBaoTri != null)
        {
            if (phieu.KeHoachBaoTri.TrangThai != TrangThaiKeHoachEnum.DangThucHien)
            {
                phieu.KeHoachBaoTri.TrangThai = TrangThaiKeHoachEnum.DangThucHien;
            }

            if (phieu.KeHoachBaoTri.ThietBi != null && phieu.KeHoachBaoTri.ThietBi.TinhTrang == TinhTrangThietBiEnum.DangHoatDong)
            {
                phieu.KeHoachBaoTri.ThietBi.TinhTrang = TinhTrangThietBiEnum.DangBaoTri;
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    /// STT 40: Chuyển phiếu sang Chờ nghiệm thu (yêu cầu có KetQuaXuLy hợp lệ)
    public async Task<ServiceResult> ChuyenChoNghiemThuAsync(
        ChuyenChoNghiemThuViewModel model,
        int? maTaiKhoanDangNhap,
        bool laAdmin
    )
    {
        if (string.IsNullOrWhiteSpace(model.KetQuaXuLy))
            return ServiceResult.Fail("Vui lòng nhập kết quả xử lý trước khi chuyển chờ nghiệm thu.", nameof(model.KetQuaXuLy));

        if (model.KetQuaXuLy.Trim().Length > 500)
            return ServiceResult.Fail("Kết quả xử lý không được vượt quá 500 ký tự.", nameof(model.KetQuaXuLy));

        var phieu = await _context.PhieuXuLys
            .Include(p => p.BaoCaoSuCo)
            .Include(p => p.KeHoachBaoTri)
            .Include(p => p.PhanCongXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == model.MaPhieuXuLy);

        if (phieu == null)
            return ServiceResult.Fail("Phiếu xử lý không tồn tại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.ChoNghiemThu)
            return ServiceResult.Fail("Phiếu này đã được chuyển sang Chờ nghiệm thu trước đó rồi.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh || phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu đã kết thúc (Hoàn thành / Đã hủy), không thể chuyển nghiệm thu.");

        if (phieu.TrangThai != TrangThaiPhieuXuLyEnum.DangXuLy)
            return ServiceResult.Fail("Chỉ phiếu đang ở trạng thái Đang xử lý mới được chuyển Chờ nghiệm thu.");

        if (!laAdmin)
        {
            if (!maTaiKhoanDangNhap.HasValue)
                return ServiceResult.Fail("Không xác định được danh tính tài khoản đăng nhập.");

            var ktv = await _phieuRepo.LayKtvTheoTaiKhoanAsync(maTaiKhoanDangNhap.Value);
            var phanCongHienTai = phieu.PhanCongXuLys.FirstOrDefault(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc);
            if (ktv == null || phanCongHienTai == null || ktv.MaKyThuatVien != phanCongHienTai.MaKyThuatVien)
                return ServiceResult.Fail("Chỉ kỹ thuật viên đang được phân công phụ trách phiếu này mới có quyền chuyển chờ nghiệm thu.");
        }

        phieu.KetQuaXuLy = model.KetQuaXuLy.Trim();
        phieu.TrangThai = TrangThaiPhieuXuLyEnum.ChoNghiemThu;

        if (phieu.LoaiXuLy == LoaiXuLyEnum.XuLySuCo && phieu.BaoCaoSuCo != null)
        {
            phieu.BaoCaoSuCo.TrangThai = TrangThaiBaoCaoEnum.ChoNghiemThu;
        }
        else if (phieu.LoaiXuLy == LoaiXuLyEnum.BaoTriDinhKy && phieu.KeHoachBaoTri != null)
        {
            phieu.KeHoachBaoTri.TrangThai = TrangThaiKeHoachEnum.ChoNghiemThu;
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    /// STT 40: Nghiệm thu phiếu (Đạt -> Hoàn thành + NgayHoanThanh; Chưa đạt -> trả về Đang xử lý; TongChiPhi hệ thống tính)
    public async Task<ServiceResult> NghiemThuAsync(
        NghiemThuPhieuViewModel model,
        int? maTaiKhoanDangNhap,
        bool laAdmin
    )
    {
        if (!laAdmin)
            return ServiceResult.Fail("Chỉ Quản trị viên (Admin) mới có quyền thực hiện nghiệm thu phiếu xử lý.");

        var phieu = await _context.PhieuXuLys
            .Include(p => p.BaoCaoSuCo).ThenInclude(b => b!.ThietBi)
            .Include(p => p.KeHoachBaoTri).ThenInclude(k => k!.ThietBi)
            .Include(p => p.PhanCongXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == model.MaPhieuXuLy);

        if (phieu == null)
            return ServiceResult.Fail("Phiếu xử lý không tồn tại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh)
            return ServiceResult.Fail("Phiếu này đã được nghiệm thu hoàn thành trước đó.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu đã bị hủy, không thể nghiệm thu.");

        if (phieu.TrangThai != TrangThaiPhieuXuLyEnum.ChoNghiemThu)
            return ServiceResult.Fail("Chỉ phiếu đang ở trạng thái Chờ nghiệm thu mới có thể nghiệm thu.");

        if (model.Dat)
        {
            var ngayHt = model.NgayHoanThanh ?? DateTime.Now;
            if (ngayHt > DateTime.Now.AddMinutes(5))
                return ServiceResult.Fail("Ngày hoàn thành thực tế không được ở trong tương lai.", nameof(model.NgayHoanThanh));

            if (phieu.NgayBatDau.HasValue && ngayHt < phieu.NgayBatDau.Value)
                return ServiceResult.Fail($"Ngày hoàn thành ({UiHelper.NgayGio(ngayHt)}) không được trước ngày bắt đầu ({UiHelper.NgayGio(phieu.NgayBatDau.Value)}).", nameof(model.NgayHoanThanh));

            phieu.TrangThai = TrangThaiPhieuXuLyEnum.HoanThanh;
            phieu.NgayHoanThanh = ngayHt;

            if (!string.IsNullOrWhiteSpace(model.NhanXetNghiemThu))
            {
                var dongLog = $"[Nghiệm thu ĐẠT {UiHelper.NgayGio(ngayHt)}]: {model.NhanXetNghiemThu.Trim()}";
                phieu.KetQuaXuLy = string.IsNullOrWhiteSpace(phieu.KetQuaXuLy)
                    ? dongLog
                    : phieu.KetQuaXuLy + Environment.NewLine + dongLog;
            }

            foreach (var pc in phieu.PhanCongXuLys.Where(x => x.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc))
            {
                pc.TrangThai = TrangThaiPhanCongEnum.DaKetThuc;
                pc.NgayKetThucPhanCong = ngayHt;
            }

            if (phieu.LoaiXuLy == LoaiXuLyEnum.XuLySuCo && phieu.BaoCaoSuCo != null)
            {
                phieu.BaoCaoSuCo.TrangThai = TrangThaiBaoCaoEnum.HoanThanh;
                if (phieu.BaoCaoSuCo.ThietBi != null && phieu.BaoCaoSuCo.ThietBi.TinhTrang != TinhTrangThietBiEnum.NgungSuDung)
                {
                    phieu.BaoCaoSuCo.ThietBi.TinhTrang = TinhTrangThietBiEnum.DangHoatDong;
                }
            }
            else if (phieu.LoaiXuLy == LoaiXuLyEnum.BaoTriDinhKy && phieu.KeHoachBaoTri != null)
            {
                phieu.KeHoachBaoTri.TrangThai = TrangThaiKeHoachEnum.HoanThanh;
                if (phieu.KeHoachBaoTri.ThietBi != null)
                {
                    var tb = phieu.KeHoachBaoTri.ThietBi;
                    if (tb.TinhTrang != TinhTrangThietBiEnum.NgungSuDung)
                    {
                        tb.TinhTrang = TinhTrangThietBiEnum.DangHoatDong;
                    }

                    if (tb.NgayBaoTriGanNhat == null || ngayHt.Date >= tb.NgayBaoTriGanNhat.Value.Date)
                    {
                        tb.NgayBaoTriGanNhat = ngayHt.Date;
                    }
                }
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(model.NhanXetNghiemThu))
                return ServiceResult.Fail("Vui lòng nhập lý do chưa đạt / yêu cầu xử lý lại để kỹ thuật viên khắc phục.", nameof(model.NhanXetNghiemThu));

            phieu.TrangThai = TrangThaiPhieuXuLyEnum.DangXuLy;
            var dongLog = $"[Nghiệm thu CHƯA ĐẠT - {UiHelper.NgayGio(DateTime.Now)}]: {model.NhanXetNghiemThu.Trim()}";
            phieu.KetQuaXuLy = string.IsNullOrWhiteSpace(phieu.KetQuaXuLy)
                ? dongLog
                : phieu.KetQuaXuLy + Environment.NewLine + dongLog;

            if (phieu.LoaiXuLy == LoaiXuLyEnum.XuLySuCo && phieu.BaoCaoSuCo != null)
            {
                phieu.BaoCaoSuCo.TrangThai = TrangThaiBaoCaoEnum.DangXuLy;
            }
            else if (phieu.LoaiXuLy == LoaiXuLyEnum.BaoTriDinhKy && phieu.KeHoachBaoTri != null)
            {
                phieu.KeHoachBaoTri.TrangThai = TrangThaiKeHoachEnum.DangThucHien;
            }
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    /// Hủy phiếu xử lý
    public async Task<ServiceResult> HuyPhieuXuLyAsync(int maPhieuXuLy, string? lyDoHuy)
    {
        var phieu = await _context.PhieuXuLys
            .Include(p => p.BaoCaoSuCo)
            .Include(p => p.KeHoachBaoTri)
            .Include(p => p.PhanCongXuLys)
            .FirstOrDefaultAsync(p => p.MaPhieuXuLy == maPhieuXuLy);

        if (phieu == null)
            return ServiceResult.Fail("Phiếu xử lý không tồn tại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh)
            return ServiceResult.Fail("Phiếu xử lý đã hoàn thành nghiệm thu, không thể hủy.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu xử lý đã bị hủy trước đó.");

        // Kết thúc tất cả các phân công còn hiệu lực
        foreach (var pc in phieu.PhanCongXuLys.Where(x => x.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc))
        {
            pc.TrangThai = TrangThaiPhanCongEnum.DaKetThuc;
            pc.NgayKetThucPhanCong = DateTime.Now;
        }

        phieu.TrangThai = TrangThaiPhieuXuLyEnum.DaHuy;
        if (!string.IsNullOrWhiteSpace(lyDoHuy))
        {
            phieu.KetQuaXuLy = $"[Đã hủy] {lyDoHuy.Trim()}";
        }

        await _context.SaveChangesAsync();
        return ServiceResult.Ok();
    }
}
