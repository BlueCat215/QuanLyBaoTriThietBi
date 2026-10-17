// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Service Kế hoạch bảo trì (validate thiết bị/ngày/nội dung/ưu tiên, chống trùng kế hoạch cùng kỳ, hủy an toàn)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class KeHoachBaoTriService : IKeHoachBaoTriService
{
    private const string KhongTimThay = "Không tìm thấy kế hoạch bảo trì.";
    private const int SoNamToiDa = 5; // chặn gõ nhầm năm (vd 2206)

    private readonly IKeHoachBaoTriRepository _repo;

    public KeHoachBaoTriService(IKeHoachBaoTriRepository repo) => _repo = repo;

    // ---------------- Truy vấn ----------------
    public async Task<KeHoachBaoTriIndexViewModel> LayDanhSachAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    ) =>
        new()
        {
            TuKhoa = tuKhoa?.Trim(),
            TrangThai = trangThai,
            MucDo = mucDo,
            Items = await _repo.TimKiemAsync(tuKhoa, trangThai, mucDo),
        };

    public async Task<ServiceResult<KeHoachBaoTriFormViewModel>> LayDeSuaAsync(int id)
    {
        var kh = await _repo.GetByIdAsync(id);
        if (kh is null)
            return ServiceResult<KeHoachBaoTriFormViewModel>.Fail(KhongTimThay);

        if (!CoTheSua(kh))
            return ServiceResult<KeHoachBaoTriFormViewModel>.Fail(
                "Chỉ sửa được kế hoạch ở trạng thái Chờ thực hiện hoặc Đã phân công."
            );

        return ServiceResult<KeHoachBaoTriFormViewModel>.Ok(
            new KeHoachBaoTriFormViewModel
            {
                MaKeHoach = kh.MaKeHoach,
                MaThietBi = kh.MaThietBi,
                NgayDuKien = kh.NgayDuKien,
                NoiDungBaoTri = kh.NoiDungBaoTri,
                MucDoUuTien = kh.MucDoUuTien,
                GhiChu = kh.GhiChu,
            }
        );
    }

    public Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(int? giuLaiMa) =>
        _repo.LayThietBiChoChonAsync(giuLaiMa);

    // ---------------- Thêm ----------------
    public async Task<ServiceResult<int>> ThemAsync(KeHoachBaoTriFormViewModel vm)
    {
        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, vm.MaThietBi, ngayCu: null, boQuaMaKeHoach: null);
        if (!loi.Succeeded)
            return Chuyen(loi);

        var kh = new KeHoachBaoTri
        {
            MaThietBi = vm.MaThietBi!.Value,
            NgayDuKien = vm.NgayDuKien!.Value.Date,
            NoiDungBaoTri = vm.NoiDungBaoTri,
            MucDoUuTien = vm.MucDoUuTien!.Value,
            TrangThai = TrangThaiKeHoachEnum.ChoThucHien, // luôn bắt đầu ở Chờ thực hiện
            GhiChu = vm.GhiChu,
        };

        await _repo.AddAsync(kh);
        await _repo.SaveChangesAsync();
        return ServiceResult<int>.Ok(kh.MaKeHoach);
    }

    // ---------------- Sửa ----------------
    public async Task<ServiceResult> SuaAsync(KeHoachBaoTriFormViewModel vm)
    {
        var kh = await _repo.GetByIdAsync(vm.MaKeHoach);
        if (kh is null)
            return ServiceResult.Fail(KhongTimThay);

        if (!CoTheSua(kh))
            return ServiceResult.Fail(
                "Chỉ sửa được kế hoạch ở trạng thái Chờ thực hiện hoặc Đã phân công."
            );

        ChuanHoa(vm);

        // Thiết bị bị khóa khi sửa: luôn dùng thiết bị gốc, bỏ qua giá trị gửi lên
        var loi = await KiemTraAsync(vm, kh.MaThietBi, ngayCu: kh.NgayDuKien, boQuaMaKeHoach: kh.MaKeHoach);
        if (!loi.Succeeded)
            return loi;

        kh.NgayDuKien = vm.NgayDuKien!.Value.Date;
        kh.NoiDungBaoTri = vm.NoiDungBaoTri;
        kh.MucDoUuTien = vm.MucDoUuTien!.Value;
        kh.GhiChu = vm.GhiChu;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Hủy ----------------
    public async Task<ServiceResult> HuyAsync(int id)
    {
        var kh = await _repo.GetByIdAsync(id);
        if (kh is null)
            return ServiceResult.Fail(KhongTimThay);

        if (kh.TrangThai == TrangThaiKeHoachEnum.DaHuy)
            return ServiceResult.Fail("Kế hoạch này đã được hủy trước đó.");

        if (!CoTheSua(kh))
            return ServiceResult.Fail(
                "Chỉ hủy được kế hoạch ở trạng thái Chờ thực hiện hoặc Đã phân công (chưa bắt đầu thực hiện)."
            );

        var soPhieu = await _repo.DemPhieuConHieuLucAsync(id);
        if (soPhieu > 0)
            return ServiceResult.Fail(
                $"Không thể hủy vì kế hoạch đang có {soPhieu} phiếu xử lý chưa hủy. Hãy hủy phiếu xử lý trước."
            );

        kh.TrangThai = TrangThaiKeHoachEnum.DaHuy;
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Business rule chống trùng kế hoạch ----------------
    public async Task<ServiceResult> KiemTraTrungKeHoachAsync(
        int maThietBi,
        DateTime ngayDuKien,
        int? boQuaMaKeHoach = null
    )
    {
        var trung = await _repo.TimKeHoachTrungKyAsync(maThietBi, ngayDuKien, boQuaMaKeHoach);
        if (trung is null)
            return ServiceResult.Ok();

        return ServiceResult.Fail(
            $"Thiết bị này đã có kế hoạch bảo trì {UiHelper.Ma("KH", trung.MaKeHoach)} "
                + $"({UiHelper.Badge(trung.TrangThai).Text}, dự kiến {UiHelper.Ngay(trung.NgayDuKien)}) "
                + $"trong tháng {ngayDuKien:MM/yyyy}. Mỗi thiết bị chỉ có 1 kế hoạch đang hoạt động trong cùng kỳ bảo trì.",
            nameof(KeHoachBaoTriFormViewModel.NgayDuKien)
        );
    }

    // ---------------- Hàm nội bộ ----------------
    private static bool CoTheSua(KeHoachBaoTri kh) =>
        kh.TrangThai is TrangThaiKeHoachEnum.ChoThucHien or TrangThaiKeHoachEnum.DaPhanCong;

    private static void ChuanHoa(KeHoachBaoTriFormViewModel vm)
    {
        vm.NoiDungBaoTri = (vm.NoiDungBaoTri ?? string.Empty).Trim();
        vm.GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim();
    }

    private static ServiceResult<int> Chuyen(ServiceResult loi)
    {
        var r = new ServiceResult<int>();
        foreach (var e in loi.Errors)
            r.AddError(e.Message, e.Field);
        return r;
    }

    /// <param name="maThietBi">Thiết bị cần kiểm tra (khi sửa là thiết bị gốc của kế hoạch).</param>
    /// <param name="ngayCu">null khi thêm mới; ngày dự kiến hiện tại khi sửa.</param>
    private async Task<ServiceResult> KiemTraAsync(
        KeHoachBaoTriFormViewModel vm,
        int? maThietBi,
        DateTime? ngayCu,
        int? boQuaMaKeHoach
    )
    {
        var kq = ServiceResult.Ok();

        // Nội dung + mức ưu tiên
        if (vm.NoiDungBaoTri.Length == 0)
            kq.AddError("Vui lòng nhập nội dung bảo trì", nameof(KeHoachBaoTriFormViewModel.NoiDungBaoTri));
        else if (vm.NoiDungBaoTri.Length > 500)
            kq.AddError("Nội dung bảo trì tối đa 500 ký tự", nameof(KeHoachBaoTriFormViewModel.NoiDungBaoTri));

        if (vm.MucDoUuTien is not { } uuTien || !Enum.IsDefined(uuTien))
            kq.AddError("Mức độ ưu tiên không hợp lệ", nameof(KeHoachBaoTriFormViewModel.MucDoUuTien));

        // Thiết bị: tồn tại và chưa Ngừng sử dụng
        ThietBi? tb = null;
        if (maThietBi is not { } maTb)
        {
            kq.AddError("Vui lòng chọn thiết bị", nameof(KeHoachBaoTriFormViewModel.MaThietBi));
        }
        else
        {
            tb = await _repo.LayThietBiAsync(maTb);
            if (tb is null)
                kq.AddError("Thiết bị không tồn tại", nameof(KeHoachBaoTriFormViewModel.MaThietBi));
            else if (tb.TinhTrang == TinhTrangThietBiEnum.NgungSuDung)
                kq.AddError(
                    "Thiết bị đã ngừng sử dụng, không thể lập hoặc sửa kế hoạch bảo trì (hãy hủy kế hoạch nếu không còn cần)",
                    nameof(KeHoachBaoTriFormViewModel.MaThietBi)
                );
        }

        // Ngày dự kiến hợp lệ
        DateTime? ngay = null;
        if (vm.NgayDuKien is not { } n)
        {
            kq.AddError("Vui lòng chọn ngày dự kiến", nameof(KeHoachBaoTriFormViewModel.NgayDuKien));
        }
        else
        {
            ngay = n.Date;
            var homNay = DateTime.Today;

            // Sửa mà không đổi ngày thì cho giữ nguyên (kể cả đã quá hạn)
            var daDoiNgay = ngayCu is null || ngayCu.Value.Date != ngay.Value;

            if (daDoiNgay && ngay < homNay)
                kq.AddError("Ngày dự kiến không được ở trong quá khứ", nameof(KeHoachBaoTriFormViewModel.NgayDuKien));
            else if (ngay > homNay.AddYears(SoNamToiDa))
                kq.AddError(
                    $"Ngày dự kiến không được quá {SoNamToiDa} năm kể từ hôm nay",
                    nameof(KeHoachBaoTriFormViewModel.NgayDuKien)
                );
            else if (tb is not null && ngay < tb.NgayDuaVaoSuDung.Date)
                kq.AddError(
                    $"Ngày dự kiến không được trước ngày đưa thiết bị vào sử dụng ({UiHelper.Ngay(tb.NgayDuaVaoSuDung)})",
                    nameof(KeHoachBaoTriFormViewModel.NgayDuKien)
                );
        }

        // Chống trùng kế hoạch: chỉ kiểm tra khi thiết bị + ngày đã hợp lệ (tránh báo lỗi chồng lỗi)
        if (kq.Succeeded && maThietBi is { } id && ngay is { } d)
        {
            var trung = await KiemTraTrungKeHoachAsync(id, d, boQuaMaKeHoach);
            if (!trung.Succeeded)
                return trung;
        }

        return kq;
    }
}
