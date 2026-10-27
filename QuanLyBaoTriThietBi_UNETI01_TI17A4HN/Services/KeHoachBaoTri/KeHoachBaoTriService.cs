// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Service Kế hoạch bảo trì (validate thiết bị/ngày/nội dung/ưu tiên, chống trùng kế hoạch cùng kỳ, hủy an toàn, nghiệm thu hoàn thành, liên kết & phát sinh Phiếu xử lý)
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
    public async Task<KeHoachBaoTriIndexViewModel> LayDanhSachAsync(KeHoachBaoTriBoLoc boLoc)
    {
        string? canhBao = null;
        if (boLoc.KhoangNgayKhongHopLe)
        {
            canhBao = "Khoảng ngày lọc không hợp lệ (Từ ngày lớn hơn Đến ngày) nên điều kiện ngày đã được bỏ qua.";
        }

        var paged = await _repo.LayPhanTrangAsync(boLoc);

        return new KeHoachBaoTriIndexViewModel
        {
            PagedItems = paged,
            BoLoc = boLoc,
            CanhBao = canhBao,
        };
    }

    public Task<KeHoachBaoTriIndexViewModel> LayDanhSachAsync(
        string? tuKhoa,
        TrangThaiKeHoachEnum? trangThai,
        MucDoUuTienEnum? mucDo
    ) =>
        LayDanhSachAsync(
            new KeHoachBaoTriBoLoc
            {
                TuKhoa = tuKhoa?.Trim(),
                TrangThai = trangThai,
                MucDo = mucDo,
            }
        );

    public async Task<ServiceResult<KeHoachBaoTriDetailsViewModel>> LayChiTietAsync(int id)
    {
        var kh = await _repo.LayChiTietAsync(id);
        if (kh is null)
            return ServiceResult<KeHoachBaoTriDetailsViewModel>.Fail(KhongTimThay);

        var phieuList = await _repo.LayPhieuXuLyTheoKeHoachAsync(id);

        return ServiceResult<KeHoachBaoTriDetailsViewModel>.Ok(
            new KeHoachBaoTriDetailsViewModel
            {
                KeHoach = kh,
                PhieuXuLys = phieuList,
            }
        );
    }

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

    // ---------------- Phát sinh Phiếu xử lý định kỳ (Liên kết Kế hoạch <-> Phiếu xử lý) ----------------
    public async Task<ServiceResult<int>> TaoPhieuXuLyDinhKyAsync(int maKeHoach)
    {
        var kh = await _repo.GetByIdAsync(maKeHoach);
        if (kh is null)
            return ServiceResult<int>.Fail(KhongTimThay);

        if (kh.TrangThai == TrangThaiKeHoachEnum.HoanThanh)
            return ServiceResult<int>.Fail("Kế hoạch này đã hoàn thành, không thể phát sinh thêm phiếu xử lý.");

        if (kh.TrangThai == TrangThaiKeHoachEnum.DaHuy)
            return ServiceResult<int>.Fail("Kế hoạch này đã bị hủy, không thể phát sinh phiếu xử lý.");

        var tb = await _repo.LayThietBiAsync(kh.MaThietBi);
        if (tb is null)
            return ServiceResult<int>.Fail("Không tìm thấy thông tin thiết bị tương ứng.");

        if (tb.TinhTrang == TinhTrangThietBiEnum.NgungSuDung)
            return ServiceResult<int>.Fail("Thiết bị đã ngừng sử dụng, không thể phát sinh phiếu xử lý bảo trì.");

        var coPhieuDangChay = await _repo.CoPhieuDangHoatDongAsync(maKeHoach);
        if (coPhieuDangChay)
            return ServiceResult<int>.Fail(
                "Kế hoạch này hiện đang có phiếu xử lý đang trong quá trình thực hiện. Mỗi kế hoạch chỉ có tối đa 1 phiếu xử lý hoạt động tại một thời điểm."
            );

        // Tạo phiếu xử lý dạng bảo trì định kỳ
        var phieu = new PhieuXuLy
        {
            LoaiXuLy = LoaiXuLyEnum.BaoTriDinhKy,
            MaKeHoach = maKeHoach,
            MaBaoCao = null, // Đúng ràng buộc CK_Phieu_Nguon
            NgayTao = DateTime.Now,
            TrangThai = TrangThaiPhieuXuLyEnum.ChoPhanCong,
        };

        await _repo.ThemPhieuXuLyAsync(phieu);

        // Chuyển trạng thái kế hoạch sang Đang thực hiện nếu đang ở Chờ thực hiện
        if (kh.TrangThai == TrangThaiKeHoachEnum.ChoThucHien)
        {
            kh.TrangThai = TrangThaiKeHoachEnum.DangThucHien;
        }

        // Chuyển tình trạng thiết bị sang Đang bảo trì nếu đang ở trạng thái Hoạt động
        if (tb.TinhTrang == TinhTrangThietBiEnum.DangHoatDong)
        {
            tb.TinhTrang = TinhTrangThietBiEnum.DangBaoTri;
        }

        await _repo.SaveChangesAsync();
        return ServiceResult<int>.Ok(phieu.MaPhieuXuLy);
    }

    // ---------------- Nghiệm thu / Hoàn thành kế hoạch & Cập nhật mốc bảo trì ----------------
    public async Task<ServiceResult> HoanThanhAsync(
        int maKeHoach,
        DateTime ngayHoanThanh,
        string? ghiChu = null
    )
    {
        var kh = await _repo.GetByIdAsync(maKeHoach);
        if (kh is null)
            return ServiceResult.Fail(KhongTimThay);

        if (kh.TrangThai == TrangThaiKeHoachEnum.HoanThanh)
            return ServiceResult.Fail("Kế hoạch này đã được hoàn thành trước đó.");

        if (kh.TrangThai == TrangThaiKeHoachEnum.DaHuy)
            return ServiceResult.Fail("Kế hoạch đã bị hủy, không thể nghiệm thu hoàn thành.");

        var tb = await _repo.LayThietBiAsync(kh.MaThietBi);
        if (tb is null)
            return ServiceResult.Fail("Không tìm thấy thiết bị tương ứng của kế hoạch.");

        var ngayHt = ngayHoanThanh.Date;
        if (ngayHt < tb.NgayDuaVaoSuDung.Date)
            return ServiceResult.Fail(
                $"Ngày hoàn thành ({UiHelper.Ngay(ngayHt)}) không được trước ngày đưa vào sử dụng ({UiHelper.Ngay(tb.NgayDuaVaoSuDung)}).",
                nameof(HoanThanhKeHoachViewModel.NgayHoanThanh)
            );

        if (ngayHt > DateTime.Today)
            return ServiceResult.Fail(
                "Ngày hoàn thành thực tế không được ở trong tương lai.",
                nameof(HoanThanhKeHoachViewModel.NgayHoanThanh)
            );

        // 1. Chuyển trạng thái kế hoạch sang Hoàn thành (bảo toàn lịch sử bản ghi)
        kh.TrangThai = TrangThaiKeHoachEnum.HoanThanh;
        if (!string.IsNullOrWhiteSpace(ghiChu))
        {
            var dongLog = $"[Nghiệm thu {UiHelper.Ngay(ngayHt)}]: {ghiChu.Trim()}";
            kh.GhiChu = string.IsNullOrWhiteSpace(kh.GhiChu)
                ? dongLog
                : $"{kh.GhiChu}\n{dongLog}";
        }

        // 2. Cập nhật mốc bảo trì gần nhất của thiết bị
        // NgayBaoTriTiepTheo là NotMapped property sẽ tự động tính lại theo mốc mới:
        // (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung).AddMonths(ChuKyBaoTriThang)
        if (tb.NgayBaoTriGanNhat == null || ngayHt >= tb.NgayBaoTriGanNhat.Value.Date)
        {
            tb.NgayBaoTriGanNhat = ngayHt;
        }

        // 3. Nếu thiết bị đang ở trạng thái DangBaoTri, phục hồi về DangHoatDong
        if (tb.TinhTrang == TinhTrangThietBiEnum.DangBaoTri)
        {
            tb.TinhTrang = TinhTrangThietBiEnum.DangHoatDong;
        }

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
