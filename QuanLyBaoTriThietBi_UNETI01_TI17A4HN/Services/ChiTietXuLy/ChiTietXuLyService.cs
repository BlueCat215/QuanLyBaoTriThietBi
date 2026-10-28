// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Service Chi tiết xử lý (STT 39: Bắt đầu xử lý cho KTV được phân công, ghi NgayBatDau, chuyển Đang xử lý; cập nhật ChiTietXuLy hình thành lịch sử xử lý; ThanhTien = SoLuong x DonGia do hệ thống tính)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class ChiTietXuLyService : IChiTietXuLyService
{
    private readonly IChiTietXuLyRepository _repo;

    public ChiTietXuLyService(IChiTietXuLyRepository repo) => _repo = repo;

    // ---------------------------------------------------------------- ĐỌC

    public async Task<ChiTietXuLyPhieuListViewModel> LayDanhSachPhieuAsync(
        TrangThaiPhieuXuLyEnum? trangThai
    ) =>
        new()
        {
            Items = await _repo.LayDanhSachPhieuAsync(trangThai),
            TrangThai = trangThai,
        };

    public async Task<ChiTietXuLyIndexViewModel?> LayTheoPhieuAsync(
        int maPhieuXuLy,
        int? maTaiKhoanDangNhap = null,
        bool laAdmin = false
    )
    {
        var phieu = await _repo.LayPhieuAsync(maPhieuXuLy);
        if (phieu is null)
            return null;

        bool laKtvPhuTrach = await LaKtvPhuTrachAsync(maPhieuXuLy, maTaiKhoanDangNhap);

        return new ChiTietXuLyIndexViewModel
        {
            Phieu = phieu,
            Items = await _repo.LayTheoPhieuAsync(maPhieuXuLy),
            TongChiPhi = await _repo.TongChiPhiAsync(maPhieuXuLy),
            LaAdmin = laAdmin,
            LaKtvPhuTrach = laKtvPhuTrach,
        };
    }

    public async Task<ChiTietXuLyFormViewModel?> LayDeThemAsync(
        int maPhieuXuLy,
        int? maTaiKhoanDangNhap = null,
        bool laAdmin = false
    )
    {
        var phieu = await _repo.LayPhieuAsync(maPhieuXuLy);
        if (phieu is null)
            return null;

        bool laKtvPhuTrach = await LaKtvPhuTrachAsync(maPhieuXuLy, maTaiKhoanDangNhap);

        return new ChiTietXuLyFormViewModel
        {
            MaPhieuXuLy = maPhieuXuLy,
            Phieu = phieu,
            VatTuOptions = await _repo.LayVatTuChoChonAsync(null),
            LaAdmin = laAdmin,
            LaKtvPhuTrach = laKtvPhuTrach,
        };
    }

    public async Task<ChiTietXuLyFormViewModel?> LayDeSuaAsync(
        int maChiTiet,
        int? maTaiKhoanDangNhap = null,
        bool laAdmin = false
    )
    {
        var ct = await _repo.GetByIdAsync(maChiTiet);
        if (ct is null)
            return null;

        bool laKtvPhuTrach = await LaKtvPhuTrachAsync(ct.MaPhieuXuLy, maTaiKhoanDangNhap);

        return new ChiTietXuLyFormViewModel
        {
            MaChiTiet = ct.MaChiTiet,
            MaPhieuXuLy = ct.MaPhieuXuLy,
            NoiDungThucHien = ct.NoiDungThucHien,
            MaVatTu = ct.MaVatTu,
            VatTuLinhKien = ct.VatTuLinhKien,
            SoLuong = ct.SoLuong,
            DonGia = ct.DonGia,
            GhiChu = ct.GhiChu,
            Phieu = await _repo.LayPhieuAsync(ct.MaPhieuXuLy),
            VatTuOptions = await _repo.LayVatTuChoChonAsync(ct.MaVatTu),
            LaAdmin = laAdmin,
            LaKtvPhuTrach = laKtvPhuTrach,
        };
    }

    public async Task NapDanhMucAsync(ChiTietXuLyFormViewModel vm)
    {
        vm.Phieu = await _repo.LayPhieuAsync(vm.MaPhieuXuLy);
        vm.VatTuOptions = await _repo.LayVatTuChoChonAsync(vm.MaVatTu);
    }

    // ---------------------------------------------------------------- GHI

    /// STT 39: Thêm dòng chi tiết (chỉ KTV đang được phân công hoặc Admin; phiếu phải ở Đang xử lý)
    public async Task<ServiceResult> ThemAsync(
        ChiTietXuLyFormViewModel vm,
        int? maTaiKhoanDangNhap = null,
        bool laAdmin = false
    )
    {
        // 1. Kiểm tra quyền thao tác
        var quyen = await KiemTraQuyenThaoTacAsync(vm.MaPhieuXuLy, maTaiKhoanDangNhap, laAdmin);
        if (!quyen.Succeeded)
            return quyen;

        var phieu = await _repo.LayPhieuAsync(vm.MaPhieuXuLy);
        if (phieu is null)
            return ServiceResult.Fail("Không tìm thấy phiếu xử lý.");
        if (!phieu.ChoPhepGhiChiTiet)
            return ServiceResult.Fail(LyDoKhongGhiDuoc(phieu));

        ChuanHoa(vm);
        var loi = await KiemTraAsync(vm, giuLaiMaVatTu: null);
        if (!loi.Succeeded)
            return loi;

        await _repo.AddAsync(
            new ChiTietXuLy
            {
                MaPhieuXuLy = vm.MaPhieuXuLy,
                NgayCapNhat = DateTime.Now, // do hệ thống xác định, không cho nhập, hình thành lịch sử xử lý
                NoiDungThucHien = vm.NoiDungThucHien,
                MaVatTu = vm.MaVatTu,
                VatTuLinhKien = vm.VatTuLinhKien,
                SoLuong = vm.SoLuong,
                DonGia = vm.DonGia,
                ThanhTien = TinhThanhTien(vm.SoLuong, vm.DonGia), // do hệ thống tính tự động = SoLuong x DonGia
                GhiChu = vm.GhiChu,
            }
        );
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    /// STT 39: Sửa dòng chi tiết (chỉ KTV đang được phân công hoặc Admin; phiếu phải ở Đang xử lý)
    public async Task<ServiceResult> SuaAsync(
        ChiTietXuLyFormViewModel vm,
        int? maTaiKhoanDangNhap = null,
        bool laAdmin = false
    )
    {
        var ct = await _repo.GetByIdAsync(vm.MaChiTiet);
        if (ct is null)
            return ServiceResult.Fail("Không tìm thấy dòng chi tiết xử lý.");

        // Mã phiếu lấy từ DB, không tin giá trị client gửi lên
        vm.MaPhieuXuLy = ct.MaPhieuXuLy;

        // 1. Kiểm tra quyền thao tác
        var quyen = await KiemTraQuyenThaoTacAsync(ct.MaPhieuXuLy, maTaiKhoanDangNhap, laAdmin);
        if (!quyen.Succeeded)
            return quyen;

        var phieu = await _repo.LayPhieuAsync(ct.MaPhieuXuLy);
        if (phieu is null)
            return ServiceResult.Fail("Không tìm thấy phiếu xử lý.");
        if (!phieu.ChoPhepGhiChiTiet)
            return ServiceResult.Fail(LyDoKhongGhiDuoc(phieu));

        ChuanHoa(vm);
        var loi = await KiemTraAsync(vm, giuLaiMaVatTu: ct.MaVatTu);
        if (!loi.Succeeded)
            return loi;

        // NgayCapNhat giữ nguyên mốc thời gian ghi nhận bước xử lý ban đầu
        ct.NoiDungThucHien = vm.NoiDungThucHien;
        ct.MaVatTu = vm.MaVatTu;
        ct.VatTuLinhKien = vm.VatTuLinhKien;
        ct.SoLuong = vm.SoLuong;
        ct.DonGia = vm.DonGia;
        ct.ThanhTien = TinhThanhTien(vm.SoLuong, vm.DonGia); // do hệ thống tính tự động
        ct.GhiChu = vm.GhiChu;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    /// STT 39: Xóa dòng chi tiết (chỉ KTV đang được phân công hoặc Admin; phiếu phải ở Đang xử lý)
    public async Task<ServiceResult<int>> XoaAsync(
        int maChiTiet,
        int? maTaiKhoanDangNhap = null,
        bool laAdmin = false
    )
    {
        var ct = await _repo.GetByIdAsync(maChiTiet);
        if (ct is null)
            return ServiceResult<int>.Fail("Không tìm thấy dòng chi tiết xử lý.");

        // 1. Kiểm tra quyền thao tác
        var quyen = await KiemTraQuyenThaoTacAsync(ct.MaPhieuXuLy, maTaiKhoanDangNhap, laAdmin);
        if (!quyen.Succeeded)
            return ServiceResult<int>.Fail(quyen.ErrorMessage ?? "Không có quyền xóa chi tiết xử lý.");

        var phieu = await _repo.LayPhieuAsync(ct.MaPhieuXuLy);
        if (phieu is null)
            return ServiceResult<int>.Fail("Không tìm thấy phiếu xử lý.");
        if (!phieu.ChoPhepGhiChiTiet)
            return ServiceResult<int>.Fail(LyDoKhongGhiDuoc(phieu));

        var maPhieu = ct.MaPhieuXuLy;
        _repo.Remove(ct);
        await _repo.SaveChangesAsync();
        return ServiceResult<int>.Ok(maPhieu);
    }

    /// <summary>
    /// STT 39: Bắt đầu xử lý (chỉ KTV đang được phân công hoặc Admin)
    /// Ghi nhận NgayBatDau, chuyển phiếu sang Đang xử lý, cập nhật trạng thái nguồn và thiết bị.
    /// </summary>
    public async Task<ServiceResult> BatDauXuLyAsync(
        int maPhieuXuLy,
        int? maTaiKhoanDangNhap,
        bool laAdmin
    )
    {
        var phieu = await _repo.LayPhieuEntityAsync(maPhieuXuLy);
        if (phieu is null)
            return ServiceResult.Fail("Không tìm thấy phiếu xử lý.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.DangXuLy)
            return ServiceResult.Fail("Phiếu này đã bắt đầu xử lý trước đó rồi.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.ChoNghiemThu)
            return ServiceResult.Fail("Phiếu đang chờ nghiệm thu, không thể bắt đầu xử lý lại.");

        if (phieu.TrangThai == TrangThaiPhieuXuLyEnum.HoanThanh || phieu.TrangThai == TrangThaiPhieuXuLyEnum.DaHuy)
            return ServiceResult.Fail("Phiếu đã kết thúc (Hoàn thành / Đã hủy), không thể bắt đầu xử lý.");

        // Tìm phân công đang có hiệu lực
        var phanCongHienTai = phieu.PhanCongXuLys
            .FirstOrDefault(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc);

        if (phanCongHienTai is null)
            return ServiceResult.Fail("Phiếu chưa được phân công cho kỹ thuật viên nào.");

        // Kiểm tra quyền: chỉ KTV đang được phân công hiệu lực (hoặc Admin) mới được bấm Bắt đầu xử lý
        if (!laAdmin)
        {
            if (!maTaiKhoanDangNhap.HasValue)
                return ServiceResult.Fail("Không xác thực được danh tính tài khoản đăng nhập.");

            var ktv = await _repo.LayKtvTheoTaiKhoanAsync(maTaiKhoanDangNhap.Value);
            if (ktv is null || ktv.MaKyThuatVien != phanCongHienTai.MaKyThuatVien)
                return ServiceResult.Fail("Chỉ kỹ thuật viên đang được phân công phụ trách phiếu này mới có quyền bấm Bắt đầu xử lý.");
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

            // Thiết bị chuyển sang Tạm ngừng do sự cố nếu đang hoạt động
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

            // Kế hoạch bảo trì: chuyển thiết bị sang Đang bảo trì nếu đang hoạt động
            if (phieu.KeHoachBaoTri.ThietBi != null && phieu.KeHoachBaoTri.ThietBi.TinhTrang == TinhTrangThietBiEnum.DangHoatDong)
            {
                phieu.KeHoachBaoTri.ThietBi.TinhTrang = TinhTrangThietBiEnum.DangBaoTri;
            }
        }

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------------------------------------------------------- NỘI BỘ

    private async Task<bool> LaKtvPhuTrachAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap)
    {
        if (!maTaiKhoanDangNhap.HasValue)
            return false;

        var ktv = await _repo.LayKtvTheoTaiKhoanAsync(maTaiKhoanDangNhap.Value);
        if (ktv is null)
            return false;

        var phanCong = await _repo.LayPhanCongHienTaiAsync(maPhieuXuLy);
        return phanCong != null && phanCong.MaKyThuatVien == ktv.MaKyThuatVien;
    }

    private async Task<ServiceResult> KiemTraQuyenThaoTacAsync(int maPhieuXuLy, int? maTaiKhoanDangNhap, bool laAdmin)
    {
        if (laAdmin)
            return ServiceResult.Ok();

        if (!maTaiKhoanDangNhap.HasValue)
            return ServiceResult.Fail("Phiên đăng nhập đã hết hạn hoặc không xác thực được tài khoản.");

        var ktv = await _repo.LayKtvTheoTaiKhoanAsync(maTaiKhoanDangNhap.Value);
        if (ktv is null)
            return ServiceResult.Fail("Tài khoản của bạn không được liên kết với hồ sơ Kỹ thuật viên.");

        var phanCong = await _repo.LayPhanCongHienTaiAsync(maPhieuXuLy);
        if (phanCong is null || phanCong.MaKyThuatVien != ktv.MaKyThuatVien)
            return ServiceResult.Fail("Chỉ kỹ thuật viên đang được phân công phụ trách phiếu này mới có quyền thực hiện thao tác.");

        return ServiceResult.Ok();
    }

    private static string LyDoKhongGhiDuoc(PhieuTomTat phieu) =>
        phieu.TrangThai switch
        {
            TrangThaiPhieuXuLyEnum.HoanThanh or TrangThaiPhieuXuLyEnum.DaHuy =>
                "Phiếu đã kết thúc (Hoàn thành / Đã hủy), không được thêm, sửa hoặc xóa chi tiết xử lý.",
            TrangThaiPhieuXuLyEnum.ChoNghiemThu =>
                "Phiếu đang chờ nghiệm thu, không được thay đổi chi tiết xử lý.",
            _ => "Phiếu chưa bắt đầu xử lý, chưa thể ghi chi tiết xử lý.",
        };

    private static void ChuanHoa(ChiTietXuLyFormViewModel vm)
    {
        vm.NoiDungThucHien = (vm.NoiDungThucHien ?? string.Empty).Trim();
        vm.VatTuLinhKien = string.IsNullOrWhiteSpace(vm.VatTuLinhKien) ? null : vm.VatTuLinhKien.Trim();
        vm.GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim();
    }

    /// STT 39: ThanhTien = SoLuong x DonGia do hệ thống tính, khớp CK_ChiTiet_ThanhTien trong DB.
    private static decimal? TinhThanhTien(int? soLuong, decimal? donGia) =>
        soLuong is { } sl && donGia is { } dg ? decimal.Round(sl * dg, 2) : null;

    /// Kiểm tra nghiệp vụ; có thể điền thêm vm.VatTuLinhKien / vm.DonGia từ vật tư đã chọn.
    private async Task<ServiceResult> KiemTraAsync(ChiTietXuLyFormViewModel vm, int? giuLaiMaVatTu)
    {
        var kq = ServiceResult.Ok();

        if (string.IsNullOrEmpty(vm.NoiDungThucHien))
            kq.AddError("Vui lòng nhập nội dung thực hiện", nameof(vm.NoiDungThucHien));

        if (vm.SoLuong is { } sl && (sl <= 0 || sl > ChiTietXuLyGioiHan.SoLuongToiDa))
            kq.AddError("Số lượng phải > 0 (tối đa 100.000)", nameof(vm.SoLuong));

        if (vm.DonGia is { } dg && (dg < 0 || dg > decimal.Parse(ChiTietXuLyGioiHan.DonGiaToiDa)))
            kq.AddError("Đơn giá phải >= 0 (tối đa 1.000.000.000)", nameof(vm.DonGia));

        // Đơn giá tính theo đồng, số nguyên
        if (vm.DonGia is { } dgLe && dgLe != decimal.Truncate(dgLe))
            kq.AddError("Đơn giá phải là số nguyên (đồng)", nameof(vm.DonGia));

        // Vật tư trong kho: phải tồn tại và còn sử dụng; điền sẵn tên và đơn giá mặc định nếu để trống.
        if (vm.MaVatTu is { } maVatTu)
        {
            var vt = await _repo.LayVatTuAsync(maVatTu);
            if (vt is null)
                kq.AddError("Vật tư đã chọn không tồn tại", nameof(vm.MaVatTu));
            else if (!vt.TrangThai && maVatTu != giuLaiMaVatTu)
                kq.AddError("Vật tư này đã ngừng sử dụng", nameof(vm.MaVatTu));
            else
            {
                vm.VatTuLinhKien ??= vt.TenVatTu;
                vm.DonGia ??= vt.DonGiaMacDinh;
            }

            if (vm.SoLuong is null)
                kq.AddError("Vui lòng nhập số lượng khi chọn vật tư", nameof(vm.SoLuong));
        }

        // Quy tắc bộ ba Vật tư - Số lượng - Đơn giá (để ThanhTien luôn tính được, không vi phạm ràng buộc DB)
        if (vm.SoLuong is not null && vm.DonGia is null)
            kq.AddError("Vui lòng nhập đơn giá (nhập 0 nếu vật tư miễn phí)", nameof(vm.DonGia));

        if (vm.DonGia is not null && vm.SoLuong is null && vm.MaVatTu is null)
            kq.AddError("Vui lòng nhập số lượng khi có đơn giá", nameof(vm.SoLuong));

        if (vm.SoLuong is not null && string.IsNullOrEmpty(vm.VatTuLinhKien))
            kq.AddError("Vui lòng nhập tên vật tư / linh kiện khi có số lượng", nameof(vm.VatTuLinhKien));

        if (vm.VatTuLinhKien is not null && vm.SoLuong is null && vm.MaVatTu is null)
            kq.AddError("Vui lòng nhập số lượng cho vật tư / linh kiện", nameof(vm.SoLuong));

        return kq;
    }
}
