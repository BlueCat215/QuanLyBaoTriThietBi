// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Service Kế hoạch bảo trì (validate thiết bị/ngày/nội dung/ưu tiên, chống trùng kế hoạch cùng kỳ, vòng đời trạng thái và chuyển trạng thái nghiêm ngặt)
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

    public async Task<ServiceResult<KeHoachBaoTriChiTietViewModel>> LayChiTietAsync(int id)
    {
        var kh = await _repo.LayChiTietTheoIdAsync(id);
        if (kh is null)
            return ServiceResult<KeHoachBaoTriChiTietViewModel>.Fail(KhongTimThay);

        var thietBi = kh.ThietBi;
        var vm = new KeHoachBaoTriChiTietViewModel
        {
            MaKeHoach = kh.MaKeHoach,
            MaThietBi = kh.MaThietBi,
            TenThietBi = thietBi?.TenThietBi ?? string.Empty,
            SoSerial = thietBi?.SoSerial ?? string.Empty,
            TenLoaiThietBi = thietBi?.LoaiThietBi?.TenLoaiThietBi ?? string.Empty,
            TenKhuVuc = thietBi?.KhuVuc?.TenKhuVuc ?? string.Empty,
            TinhTrangThietBi = thietBi?.TinhTrang ?? TinhTrangThietBiEnum.DangHoatDong,
            NgayBaoTriGanNhat = thietBi?.NgayBaoTriGanNhat,
            NgayBaoTriTiepTheo = thietBi != null ? thietBi.NgayBaoTriTiepTheo : kh.NgayDuKien,
            NgayDuKien = kh.NgayDuKien,
            NoiDungBaoTri = kh.NoiDungBaoTri,
            MucDoUuTien = kh.MucDoUuTien,
            TrangThai = kh.TrangThai,
            GhiChu = kh.GhiChu,
            DanhSachPhieuXuLy = kh.PhieuXuLys.Select(p => new KeHoachPhieuXuLyItem
            {
                MaPhieuXuLy = p.MaPhieuXuLy,
                LoaiXuLy = p.LoaiXuLy,
                NgayTao = p.NgayTao,
                NgayBatDau = p.NgayBatDau,
                NgayHoanThanh = p.NgayHoanThanh,
                TrangThai = p.TrangThai,
                KetQuaXuLy = p.KetQuaXuLy,
                KyThuatVienHienTai = p.PhanCongXuLys
                    .Where(pc => pc.TrangThai == TrangThaiPhanCongEnum.DangHieuLuc)
                    .Select(pc => pc.KyThuatVien != null ? pc.KyThuatVien.HoTen : null)
                    .FirstOrDefault(),
            }).ToList(),
        };

        return ServiceResult<KeHoachBaoTriChiTietViewModel>.Ok(vm);
    }

    public async Task<ServiceResult<KeHoachBaoTriFormViewModel>> LayDeSuaAsync(int id)
    {
        var kh = await _repo.GetByIdAsync(id);
        if (kh is null)
            return ServiceResult<KeHoachBaoTriFormViewModel>.Fail(KhongTimThay);

        if (KeHoachWorkflowHelper.IsLocked(kh.TrangThai))
            return ServiceResult<KeHoachBaoTriFormViewModel>.Fail(
                $"Kế hoạch đã {KeHoachWorkflowHelper.GetDisplayName(kh.TrangThai).ToLower()} (bị khóa), không thể chỉnh sửa."
            );

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
            TrangThai = TrangThaiKeHoachEnum.ChoThucHien, // luôn bắt đầu ở Chờ thực hiện theo đúng vòng đời
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

        // Khóa chặn cập nhật khi kế hoạch đã Hoàn thành hoặc Đã hủy (STT 31)
        if (KeHoachWorkflowHelper.IsLocked(kh.TrangThai))
            return ServiceResult.Fail(
                $"Kế hoạch đã {KeHoachWorkflowHelper.GetDisplayName(kh.TrangThai).ToLower()} (bị khóa), không thể chỉnh sửa."
            );

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
        return await ChuyenTrangThaiAsync(id, TrangThaiKeHoachEnum.DaHuy, "Hủy kế hoạch bảo trì");
    }

    // ---------------- Vòng đời trạng thái Kế hoạch bảo trì (STT 31) ----------------
    public async Task<ServiceResult> ChuyenTrangThaiAsync(
        int id,
        TrangThaiKeHoachEnum trangThaiMoi,
        string? ghiChu = null
    )
    {
        var kh = await _repo.GetByIdAsync(id);
        if (kh is null)
            return ServiceResult.Fail(KhongTimThay);

        if (kh.TrangThai == trangThaiMoi)
            return ServiceResult.Ok();

        // 1. Kiểm tra trạng thái hiện tại đã bị khóa chưa (Hoàn thành hoặc Đã hủy)
        if (KeHoachWorkflowHelper.IsLocked(kh.TrangThai))
            return ServiceResult.Fail(
                $"Kế hoạch đã {KeHoachWorkflowHelper.GetDisplayName(kh.TrangThai).ToLower()} (bị khóa), không thể thay đổi trạng thái."
            );

        // 2. Kiểm tra điều kiện chuyển đổi trạng thái hợp lệ theo đúng luồng
        // Chờ thực hiện -> Đã phân công -> Đang thực hiện -> Chờ nghiệm thu -> Hoàn thành / Đã hủy
        if (!KeHoachWorkflowHelper.CanTransitionTo(kh.TrangThai, trangThaiMoi))
        {
            return ServiceResult.Fail(
                $"Không thể chuyển trạng thái từ '{KeHoachWorkflowHelper.GetDisplayName(kh.TrangThai)}' " +
                $"sang '{KeHoachWorkflowHelper.GetDisplayName(trangThaiMoi)}' theo quy trình nghiệp vụ."
            );
        }

        // 3. Quy tắc riêng khi HỦY: Không hủy nếu còn phiếu xử lý đang hoạt động
        if (trangThaiMoi == TrangThaiKeHoachEnum.DaHuy)
        {
            var soPhieu = await _repo.DemPhieuConHieuLucAsync(id);
            if (soPhieu > 0)
                return ServiceResult.Fail(
                    $"Không thể hủy vì kế hoạch đang có {soPhieu} phiếu xử lý chưa hủy. Hãy hủy phiếu xử lý trước."
                );
        }

        // 4. Quy tắc khi ĐANG THỰC HIỆN: Cập nhật thiết bị sang Đang bảo trì nếu đang hoạt động bình thường
        if (trangThaiMoi == TrangThaiKeHoachEnum.DangThucHien)
        {
            var thietBi = await _repo.LayThietBiCoTheoDoiAsync(kh.MaThietBi);
            if (thietBi != null && thietBi.TinhTrang == TinhTrangThietBiEnum.DangHoatDong)
            {
                thietBi.TinhTrang = TinhTrangThietBiEnum.DangBaoTri;
            }
        }

        // 5. Quy tắc khi HOÀN THÀNH (STT 31 + STT 32):
        // Cập nhật NgayBaoTriGanNhat = ngày hoàn thành (hôm nay), chuyển thiết bị về Đang hoạt động
        if (trangThaiMoi == TrangThaiKeHoachEnum.HoanThanh)
        {
            var thietBi = await _repo.LayThietBiCoTheoDoiAsync(kh.MaThietBi);
            if (thietBi != null)
            {
                thietBi.NgayBaoTriGanNhat = DateTime.Today;
                if (thietBi.TinhTrang == TinhTrangThietBiEnum.DangBaoTri)
                {
                    thietBi.TinhTrang = TinhTrangThietBiEnum.DangHoatDong;
                }
            }
        }

        // 6. Ghi nhận nhật ký trạng thái vào Ghi chú nếu có
        if (!string.IsNullOrWhiteSpace(ghiChu))
        {
            var dongNhatKy = $"[{DateTime.Now:dd/MM/yyyy HH:mm} - {KeHoachWorkflowHelper.GetDisplayName(trangThaiMoi)}]: {ghiChu.Trim()}";
            kh.GhiChu = string.IsNullOrWhiteSpace(kh.GhiChu) ? dongNhatKy : $"{kh.GhiChu}\n{dongNhatKy}";
        }

        kh.TrangThai = trangThaiMoi;
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
        !KeHoachWorkflowHelper.IsLocked(kh.TrangThai)
        && kh.TrangThai is TrangThaiKeHoachEnum.ChoThucHien or TrangThaiKeHoachEnum.DaPhanCong;

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
        if (maThietBi is not { } id || id <= 0)
            return ServiceResult.Fail(
                "Vui lòng chọn thiết bị hợp lệ.",
                nameof(KeHoachBaoTriFormViewModel.MaThietBi)
            );

        var thietBi = await _repo.LayThietBiAsync(id);
        if (thietBi is null)
            return ServiceResult.Fail(
                "Thiết bị được chọn không tồn tại trên hệ thống.",
                nameof(KeHoachBaoTriFormViewModel.MaThietBi)
            );

        if (thietBi.TinhTrang == TinhTrangThietBiEnum.NgungSuDung)
            return ServiceResult.Fail(
                $"Thiết bị '{thietBi.TenThietBi}' đã ngừng sử dụng, không thể lập/sửa kế hoạch bảo trì.",
                nameof(KeHoachBaoTriFormViewModel.MaThietBi)
            );

        if (vm.NgayDuKien is not { } ngay)
            return ServiceResult.Fail(
                "Vui lòng chọn ngày dự kiến bảo trì.",
                nameof(KeHoachBaoTriFormViewModel.NgayDuKien)
            );

        var homNay = DateTime.Today;
        var gioiHanTren = homNay.AddYears(SoNamToiDa);

        if (ngay.Date > gioiHanTren)
            return ServiceResult.Fail(
                $"Ngày dự kiến không được vượt quá {SoNamToiDa} năm ({gioiHanTren:dd/MM/yyyy}).",
                nameof(KeHoachBaoTriFormViewModel.NgayDuKien)
            );

        var thayDoiNgay = ngayCu is null || ngay.Date != ngayCu.Value.Date;
        if (thayDoiNgay && ngay.Date < homNay)
            return ServiceResult.Fail(
                "Ngày dự kiến bảo trì không thể ở quá khứ.",
                nameof(KeHoachBaoTriFormViewModel.NgayDuKien)
            );

        if (thayDoiNgay)
        {
            var trung = await KiemTraTrungKeHoachAsync(id, ngay, boQuaMaKeHoach);
            if (!trung.Succeeded)
                return trung;
        }

        return ServiceResult.Ok();
    }
}
