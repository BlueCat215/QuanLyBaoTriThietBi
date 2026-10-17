// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Service Chi tiết xử lý (validate nghiệp vụ, ThanhTien = SoLuong x DonGia do hệ thống tính, chặn khi phiếu không còn đang xử lý)
#nullable enable
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

    public async Task<ChiTietXuLyIndexViewModel?> LayTheoPhieuAsync(int maPhieuXuLy)
    {
        var phieu = await _repo.LayPhieuAsync(maPhieuXuLy);
        if (phieu is null)
            return null;

        return new ChiTietXuLyIndexViewModel
        {
            Phieu = phieu,
            Items = await _repo.LayTheoPhieuAsync(maPhieuXuLy),
            TongChiPhi = await _repo.TongChiPhiAsync(maPhieuXuLy),
        };
    }

    public async Task<ChiTietXuLyFormViewModel?> LayDeThemAsync(int maPhieuXuLy)
    {
        var phieu = await _repo.LayPhieuAsync(maPhieuXuLy);
        if (phieu is null)
            return null;

        return new ChiTietXuLyFormViewModel
        {
            MaPhieuXuLy = maPhieuXuLy,
            Phieu = phieu,
            VatTuOptions = await _repo.LayVatTuChoChonAsync(null),
        };
    }

    public async Task<ChiTietXuLyFormViewModel?> LayDeSuaAsync(int maChiTiet)
    {
        var ct = await _repo.GetByIdAsync(maChiTiet);
        if (ct is null)
            return null;

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
        };
    }

    public async Task NapDanhMucAsync(ChiTietXuLyFormViewModel vm)
    {
        vm.Phieu = await _repo.LayPhieuAsync(vm.MaPhieuXuLy);
        vm.VatTuOptions = await _repo.LayVatTuChoChonAsync(vm.MaVatTu);
    }

    // ---------------------------------------------------------------- GHI

    public async Task<ServiceResult> ThemAsync(ChiTietXuLyFormViewModel vm)
    {
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
                NgayCapNhat = DateTime.Now, // do hệ thống xác định, không cho nhập
                NoiDungThucHien = vm.NoiDungThucHien,
                MaVatTu = vm.MaVatTu,
                VatTuLinhKien = vm.VatTuLinhKien,
                SoLuong = vm.SoLuong,
                DonGia = vm.DonGia,
                ThanhTien = TinhThanhTien(vm.SoLuong, vm.DonGia),
                GhiChu = vm.GhiChu,
            }
        );
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> SuaAsync(ChiTietXuLyFormViewModel vm)
    {
        var ct = await _repo.GetByIdAsync(vm.MaChiTiet);
        if (ct is null)
            return ServiceResult.Fail("Không tìm thấy dòng chi tiết xử lý.");

        // Mã phiếu lấy từ DB, không tin giá trị client gửi lên (không cho chuyển dòng sang phiếu khác).
        vm.MaPhieuXuLy = ct.MaPhieuXuLy;

        var phieu = await _repo.LayPhieuAsync(ct.MaPhieuXuLy);
        if (phieu is null)
            return ServiceResult.Fail("Không tìm thấy phiếu xử lý.");
        if (!phieu.ChoPhepGhiChiTiet)
            return ServiceResult.Fail(LyDoKhongGhiDuoc(phieu));

        ChuanHoa(vm);
        var loi = await KiemTraAsync(vm, giuLaiMaVatTu: ct.MaVatTu);
        if (!loi.Succeeded)
            return loi;

        // NgayCapNhat giữ nguyên: đó là thời điểm ghi nhận bước xử lý, dùng cho lịch sử xử lý (việc 41).
        ct.NoiDungThucHien = vm.NoiDungThucHien;
        ct.MaVatTu = vm.MaVatTu;
        ct.VatTuLinhKien = vm.VatTuLinhKien;
        ct.SoLuong = vm.SoLuong;
        ct.DonGia = vm.DonGia;
        ct.ThanhTien = TinhThanhTien(vm.SoLuong, vm.DonGia);
        ct.GhiChu = vm.GhiChu;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<int>> XoaAsync(int maChiTiet)
    {
        var ct = await _repo.GetByIdAsync(maChiTiet);
        if (ct is null)
            return ServiceResult<int>.Fail("Không tìm thấy dòng chi tiết xử lý.");

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

    // ---------------------------------------------------------------- NỘI BỘ

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

    /// ThanhTien = SoLuong x DonGia; chỉ có khi đủ cả hai (khớp CK_ChiTiet_ThanhTien trong DB).
    private static decimal? TinhThanhTien(int? soLuong, decimal? donGia) =>
        soLuong is { } sl && donGia is { } dg ? decimal.Round(sl * dg, 2) : null;

    /// Kiểm tra nghiệp vụ; có thể điền thêm vm.VatTuLinhKien / vm.DonGia từ vật tư đã chọn.
    /// giuLaiMaVatTu: vật tư hiện tại của dòng đang sửa - được giữ nguyên dù vật tư đó đã ngừng sử dụng.
    private async Task<ServiceResult> KiemTraAsync(ChiTietXuLyFormViewModel vm, int? giuLaiMaVatTu)
    {
        var kq = ServiceResult.Ok();

        if (string.IsNullOrEmpty(vm.NoiDungThucHien))
            kq.AddError("Vui lòng nhập nội dung thực hiện", nameof(vm.NoiDungThucHien));

        if (vm.SoLuong is { } sl && (sl <= 0 || sl > ChiTietXuLyGioiHan.SoLuongToiDa))
            kq.AddError("Số lượng phải > 0 (tối đa 100.000)", nameof(vm.SoLuong));

        if (vm.DonGia is { } dg && (dg < 0 || dg > decimal.Parse(ChiTietXuLyGioiHan.DonGiaToiDa)))
            kq.AddError("Đơn giá phải >= 0 (tối đa 1.000.000.000)", nameof(vm.DonGia));

        // Đơn giá tính theo đồng, số nguyên (cũng tránh sai lệch dấu thập phân theo culture khi bind form)
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
