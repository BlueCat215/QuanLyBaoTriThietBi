// Họ tên: Nguyễn Minh Đức - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 2: Quản lý thiết bị - ThietBiService
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class ThietBiService : IThietBiService
{
    private static readonly DateTime NgayToiThieu = new(1900, 1, 1);

    private readonly IThietBiRepository _repo;
    private readonly ITepDinhKemService _tepDinhKemService;

    public ThietBiService(IThietBiRepository repo, ITepDinhKemService tepDinhKemService)
    {
        _repo = repo;
        _tepDinhKemService = tepDinhKemService;
    }

    public async Task<ThietBiIndexViewModel> LayDanhSachAsync(ThietBiBoLoc boLoc)
    {
        boLoc.TuKhoa = boLoc.TuKhoa?.Trim();

        var paged = await _repo.LayPhanTrangAsync(boLoc);

        var pagedRows = paged.Map(t => new ThietBiRow(
            t.MaThietBi,
            t.TenThietBi,
            t.SoSerial,
            t.LoaiThietBi?.TenLoaiThietBi ?? "—",
            t.KhuVuc?.TenKhuVuc ?? "—",
            t.TinhTrang,
            t.NgayBaoTriTiepTheo
        ));

        return new ThietBiIndexViewModel
        {
            BoLoc = boLoc,
            PagedItems = pagedRows,
            LoaiDeLoc = await _repo.LayLoaiDeLocAsync(),
            KhuVucDeLoc = await _repo.LayKhuVucDeLocAsync(),
            CanhBao = boLoc.KhoangNgayKhongHopLe
                ? "Khoảng ngày không hợp lệ: \"Từ ngày\" lớn hơn \"Đến ngày\" nên khoảng ngày đã được bỏ qua."
                : null,
        };
    }

    public async Task<List<ThietBiRow>> LayToanBoDeXuatExcelAsync(ThietBiBoLoc boLoc)
    {
        boLoc.TuKhoa = boLoc.TuKhoa?.Trim();
        var danhSach = await _repo.TimKiemAsync(boLoc);
        return danhSach.Select(t => new ThietBiRow(
            t.MaThietBi,
            t.TenThietBi,
            t.SoSerial,
            t.LoaiThietBi?.TenLoaiThietBi ?? "—",
            t.KhuVuc?.TenKhuVuc ?? "—",
            t.TinhTrang,
            t.NgayBaoTriTiepTheo
        )).ToList();
    }

    public async Task<(int SapDenHan, int QuaHan)> DemHanBaoTriAsync(DateTime? homNay = null)
    {
        var sapDenHan = await _repo.DemSapDenHanAsync(homNay);
        var quaHan = await _repo.DemQuaHanAsync(homNay);
        return (sapDenHan, quaHan);
    }

    public async Task<List<ThietBiRow>> LayDanhSachSapDenHanAsync(int top = 5, DateTime? homNay = null)
    {
        var ds = await _repo.LayDanhSachSapDenHanAsync(top, homNay);
        return ds.Select(t => new ThietBiRow(
            t.MaThietBi,
            t.TenThietBi,
            t.SoSerial,
            t.LoaiThietBi?.TenLoaiThietBi ?? "—",
            t.KhuVuc?.TenKhuVuc ?? "—",
            t.TinhTrang,
            t.NgayBaoTriTiepTheo
        )).ToList();
    }

    public async Task<List<ThietBiRow>> LayDanhSachQuaHanAsync(int top = 5, DateTime? homNay = null)
    {
        var ds = await _repo.LayDanhSachQuaHanAsync(top, homNay);
        return ds.Select(t => new ThietBiRow(
            t.MaThietBi,
            t.TenThietBi,
            t.SoSerial,
            t.LoaiThietBi?.TenLoaiThietBi ?? "—",
            t.KhuVuc?.TenKhuVuc ?? "—",
            t.TinhTrang,
            t.NgayBaoTriTiepTheo
        )).ToList();
    }

    public async Task<ThietBiDetailsViewModel?> LayChiTietAsync(int id)
    {
        var tb = await _repo.LayChiTietAsync(id);
        if (tb is null)
            return null;

        return new ThietBiDetailsViewModel
        {
            ThietBi = tb,
            DuLieuLienQuan = await _repo.DemDuLieuLienQuanAsync(id),
            DanhSachAnh = await _tepDinhKemService.LayDanhSachTheoDoiTuongAsync(LoaiDoiTuongDinhKemEnum.ThietBi, id),
        };
    }

    public async Task<ThietBiFormViewModel?> LayDeSuaAsync(int id)
    {
        var tb = await _repo.GetByIdAsync(id);
        if (tb is null)
            return null;

        return new ThietBiFormViewModel
        {
            MaThietBi = tb.MaThietBi,
            TenThietBi = tb.TenThietBi,
            MaLoaiThietBi = tb.MaLoaiThietBi,
            MaKhuVuc = tb.MaKhuVuc,
            SoSerial = tb.SoSerial,
            NgayDuaVaoSuDung = tb.NgayDuaVaoSuDung,
            ChuKyBaoTriThang = tb.ChuKyBaoTriThang,
            NgayBaoTriGanNhat = tb.NgayBaoTriGanNhat,
            MoTa = tb.MoTa,
            DanhSachAnhHienTai = await _tepDinhKemService.LayDanhSachTheoDoiTuongAsync(LoaiDoiTuongDinhKemEnum.ThietBi, id),
        };
    }

    public Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa) =>
        _repo.LayLoaiChoChonAsync(giuLaiMa);

    public Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa) =>
        _repo.LayKhuVucChoChonAsync(giuLaiMa);

    public async Task<ServiceResult<int>> ThemAsync(ThietBiFormViewModel vm, int? maTaiKhoan = null)
    {
        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, hienTai: null);
        if (!loi.Succeeded)
            return ServiceResult<int>.Fail(loi.ErrorMessage);

        var tb = new ThietBi
        {
            TenThietBi = vm.TenThietBi,
            MaLoaiThietBi = vm.MaLoaiThietBi!.Value,
            MaKhuVuc = vm.MaKhuVuc!.Value,
            SoSerial = vm.SoSerial,
            NgayDuaVaoSuDung = vm.NgayDuaVaoSuDung!.Value.Date,
            ChuKyBaoTriThang = vm.ChuKyBaoTriThang!.Value,
            NgayBaoTriGanNhat = vm.NgayBaoTriGanNhat?.Date,
            TinhTrang = TinhTrangThietBiEnum.DangHoatDong,
            MoTa = vm.MoTa,
        };

        await _repo.AddAsync(tb);
        await _repo.SaveChangesAsync();

        if (vm.TepAnhDinhKem != null && vm.TepAnhDinhKem.Any(f => f.Length > 0))
        {
            await _tepDinhKemService.UploadNhieuTepAsync(
                vm.TepAnhDinhKem,
                LoaiDoiTuongDinhKemEnum.ThietBi,
                tb.MaThietBi,
                maTaiKhoan,
                chiChapNhanAnh: true
            );
        }

        return ServiceResult<int>.Ok(tb.MaThietBi);
    }

    public async Task<ServiceResult> SuaAsync(ThietBiFormViewModel vm, int? maTaiKhoan = null)
    {
        var tb = await _repo.GetByIdAsync(vm.MaThietBi);
        if (tb is null)
            return ServiceResult.Fail("Không tìm thấy thiết bị.");

        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, tb);
        if (!loi.Succeeded)
            return loi;

        tb.TenThietBi = vm.TenThietBi;
        tb.MaLoaiThietBi = vm.MaLoaiThietBi!.Value;
        tb.MaKhuVuc = vm.MaKhuVuc!.Value;
        tb.SoSerial = vm.SoSerial;
        tb.NgayDuaVaoSuDung = vm.NgayDuaVaoSuDung!.Value.Date;
        tb.ChuKyBaoTriThang = vm.ChuKyBaoTriThang!.Value;
        tb.NgayBaoTriGanNhat = vm.NgayBaoTriGanNhat?.Date;
        tb.MoTa = vm.MoTa;

        await _repo.SaveChangesAsync();

        if (vm.TepAnhDinhKem != null && vm.TepAnhDinhKem.Any(f => f.Length > 0))
        {
            await _tepDinhKemService.UploadNhieuTepAsync(
                vm.TepAnhDinhKem,
                LoaiDoiTuongDinhKemEnum.ThietBi,
                tb.MaThietBi,
                maTaiKhoan,
                chiChapNhanAnh: true
            );
        }

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult<TinhTrangThietBiEnum>> DoiTinhTrangAsync(
        int id,
        TinhTrangThietBiEnum tinhTrangMoi
    )
    {
        if (!Enum.IsDefined(tinhTrangMoi))
            return ServiceResult<TinhTrangThietBiEnum>.Fail("Tình trạng không hợp lệ.");

        var tb = await _repo.GetByIdAsync(id);
        if (tb is null)
            return ServiceResult<TinhTrangThietBiEnum>.Fail("Không tìm thấy thiết bị.");

        if (tb.TinhTrang != tinhTrangMoi)
        {
            tb.TinhTrang = tinhTrangMoi;
            await _repo.SaveChangesAsync();
        }
        return ServiceResult<TinhTrangThietBiEnum>.Ok(tinhTrangMoi);
    }

    public async Task<ServiceResult> XoaAsync(int id)
    {
        var tb = await _repo.GetByIdAsync(id);
        if (tb is null)
            return ServiceResult.Fail("Không tìm thấy thiết bị.");

        var lienQuan = await _repo.DemDuLieuLienQuanAsync(id);
        if (lienQuan.CoDuLieu)
            return ServiceResult.Fail(
                $"Không thể xóa thiết bị \"{tb.TenThietBi}\" vì đã phát sinh dữ liệu ({lienQuan.SoKeHoach} kế hoạch bảo trì, {lienQuan.SoBaoCao} báo cáo sự cố, "
                    + $"{lienQuan.SoThongBao} thông báo, {lienQuan.SoTepDinhKem} tệp đính kèm). Hãy chuyển thiết bị sang Ngừng sử dụng để giữ lịch sử."
            );

        try
        {
            _repo.Remove(tb);
            await _repo.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail(
                "Không thể xóa: thiết bị còn dữ liệu liên quan. Hãy chuyển sang Ngừng sử dụng."
            );
        }
        return ServiceResult.Ok();
    }

    private static void ChuanHoa(ThietBiFormViewModel vm)
    {
        vm.TenThietBi = (vm.TenThietBi ?? string.Empty).Trim();
        vm.SoSerial = (vm.SoSerial ?? string.Empty).Trim();
        vm.MoTa = string.IsNullOrWhiteSpace(vm.MoTa) ? null : vm.MoTa.Trim();
    }

    private async Task<ServiceResult> KiemTraAsync(ThietBiFormViewModel vm, ThietBi? hienTai)
    {
        if (
            vm.NgayDuaVaoSuDung is null
            || vm.ChuKyBaoTriThang is null
            || vm.MaLoaiThietBi is null
            || vm.MaKhuVuc is null
        )
            return ServiceResult.Fail("Vui lòng nhập đầy đủ thông tin bắt buộc.");

        var kq = ServiceResult.Ok();
        var homNay = DateTime.Today;

        if (await _repo.SoSerialDaTonTaiAsync(vm.SoSerial, hienTai?.MaThietBi))
            kq.AddError("Số serial đã tồn tại", nameof(ThietBiFormViewModel.SoSerial));

        var ngaySuDung = vm.NgayDuaVaoSuDung.Value.Date;
        if (ngaySuDung > homNay)
            kq.AddError(
                "Ngày đưa vào sử dụng không được lớn hơn ngày hiện tại",
                nameof(ThietBiFormViewModel.NgayDuaVaoSuDung)
            );
        else if (ngaySuDung < NgayToiThieu)
            kq.AddError(
                "Ngày đưa vào sử dụng không hợp lệ",
                nameof(ThietBiFormViewModel.NgayDuaVaoSuDung)
            );

        if (vm.NgayBaoTriGanNhat is { } baoTri)
        {
            if (baoTri.Date > homNay)
                kq.AddError(
                    "Ngày bảo trì gần nhất không được lớn hơn ngày hiện tại",
                    nameof(ThietBiFormViewModel.NgayBaoTriGanNhat)
                );
            else if (baoTri.Date < ngaySuDung)
                kq.AddError(
                    "Ngày bảo trì gần nhất không được trước ngày đưa vào sử dụng",
                    nameof(ThietBiFormViewModel.NgayBaoTriGanNhat)
                );
        }

        if (vm.ChuKyBaoTriThang <= 0)
            kq.AddError(
                "Chu kỳ bảo trì phải lớn hơn 0 (tháng)",
                nameof(ThietBiFormViewModel.ChuKyBaoTriThang)
            );

        if (
            vm.MaLoaiThietBi != hienTai?.MaLoaiThietBi
            && !await _repo.LoaiDangHoatDongAsync(vm.MaLoaiThietBi.Value)
        )
            kq.AddError("Loại thiết bị đã ngưng sử dụng", nameof(ThietBiFormViewModel.MaLoaiThietBi));

        if (
            vm.MaKhuVuc != hienTai?.MaKhuVuc
            && !await _repo.KhuVucDangHoatDongAsync(vm.MaKhuVuc.Value)
        )
            kq.AddError("Khu vực đã ngưng sử dụng", nameof(ThietBiFormViewModel.MaKhuVuc));

        return kq;
    }
}
