// Họ tên: Nguyễn Minh Đức
// Nội dung:  Service Thiết bị (CRUD, SFSP, hạn bảo trì, không trùng serial, ngày sử dụng <= hôm nay, chu kỳ > 0, đổi tình trạng, xóa an toàn, phân trang)
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

    public ThietBiService(IThietBiRepository repo) => _repo = repo;

    // ---------------- Truy vấn ----------------
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
        };
    }

    public Task<List<(int Ma, string Ten)>> LayLoaiChoChonAsync(int? giuLaiMa) =>
        _repo.LayLoaiChoChonAsync(giuLaiMa);

    public Task<List<(int Ma, string Ten)>> LayKhuVucChoChonAsync(int? giuLaiMa) =>
        _repo.LayKhuVucChoChonAsync(giuLaiMa);

    // ---------------- Thêm ----------------
    public async Task<ServiceResult> ThemAsync(ThietBiFormViewModel vm)
    {
        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, hienTai: null);
        if (!loi.Succeeded)
            return loi;

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
        return ServiceResult.Ok();
    }

    // ---------------- Sửa ----------------
    public async Task<ServiceResult> SuaAsync(ThietBiFormViewModel vm)
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
        return ServiceResult.Ok();
    }

    // ---------------- Đổi tình trạng ----------------
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

    // ---------------- Xóa an toàn ----------------
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

    // ---------------- Hàm nội bộ ----------------
    private static void ChuanHoa(ThietBiFormViewModel vm)
    {
        vm.TenThietBi = (vm.TenThietBi ?? string.Empty).Trim();
        vm.SoSerial = (vm.SoSerial ?? string.Empty).Trim();
        vm.MoTa = string.IsNullOrWhiteSpace(vm.MoTa) ? null : vm.MoTa.Trim();
    }

    /// <param name="hienTai">null khi thêm mới; là thiết bị đang sửa khi cập nhật.</param>
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

        // Serial không trùng
        if (await _repo.SoSerialDaTonTaiAsync(vm.SoSerial, hienTai?.MaThietBi))
            kq.AddError("Số serial đã tồn tại", nameof(ThietBiFormViewModel.SoSerial));

        // Ngày đưa vào sử dụng <= hiện tại
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

        // Ngày bảo trì gần nhất (tùy chọn)
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

        // Chu kỳ bảo trì > 0
        if (vm.ChuKyBaoTriThang <= 0)
            kq.AddError(
                "Chu kỳ bảo trì phải lớn hơn 0 (tháng)",
                nameof(ThietBiFormViewModel.ChuKyBaoTriThang)
            );

        // Loại / Khu vực phải tồn tại và còn hoạt động (giữ nguyên nếu thiết bị đang thuộc loại/khu vực đó)
        if (
            vm.MaLoaiThietBi != hienTai?.MaLoaiThietBi
            && !await _repo.LoaiDangHoatDongAsync(vm.MaLoaiThietBi.Value)
        )
            kq.AddError(
                "Loại thiết bị không tồn tại hoặc đã ngừng sử dụng",
                nameof(ThietBiFormViewModel.MaLoaiThietBi)
            );

        if (
            vm.MaKhuVuc != hienTai?.MaKhuVuc
            && !await _repo.KhuVucDangHoatDongAsync(vm.MaKhuVuc.Value)
        )
            kq.AddError(
                "Khu vực không tồn tại hoặc đã ngừng hoạt động",
                nameof(ThietBiFormViewModel.MaKhuVuc)
            );

        return kq;
    }
}
