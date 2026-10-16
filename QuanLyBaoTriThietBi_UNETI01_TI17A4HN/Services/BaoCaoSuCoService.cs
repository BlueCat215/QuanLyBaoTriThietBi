// Họ tên: Trịnh Trung Hoàng
// MSSV: 23103100229
// Nội dung: Module 4 - Service Báo cáo sự cố (validate thiết bị/mô tả/mức độ, ngày do hệ thống gán, phạm vi xem, hủy/xóa an toàn)
#nullable enable
using Microsoft.EntityFrameworkCore;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

public class BaoCaoSuCoService : IBaoCaoSuCoService
{
    private const string KhongTimThay = "Không tìm thấy báo cáo sự cố.";

    private readonly IBaoCaoSuCoRepository _repo;

    public BaoCaoSuCoService(IBaoCaoSuCoRepository repo) => _repo = repo;

    // ---------------- Truy vấn ----------------
    public async Task<BaoCaoSuCoIndexViewModel> LayDanhSachAsync(
        string? tuKhoa,
        TrangThaiBaoCaoEnum? trangThai,
        MucDoSuCoEnum? mucDo,
        int? maTaiKhoanLoc
    ) =>
        new()
        {
            TuKhoa = tuKhoa?.Trim(),
            TrangThai = trangThai,
            MucDo = mucDo,
            LaCuaToi = maTaiKhoanLoc.HasValue,
            Items = await _repo.TimKiemAsync(tuKhoa, trangThai, mucDo, maTaiKhoanLoc),
        };

    public async Task<BaoCaoSuCoDetailsViewModel?> LayChiTietAsync(int id, NguoiThaoTac nguoi)
    {
        var bc = await _repo.LayChiTietAsync(id);
        if (bc is null || !DuocXem(bc, nguoi))
            return null;

        var lienQuan = await _repo.DemDuLieuLienQuanAsync(id);
        return new BaoCaoSuCoDetailsViewModel
        {
            BaoCao = bc,
            DuLieuLienQuan = lienQuan,
            PhieuXuLys = await _repo.LayPhieuXuLyAsync(id),
            LaAdmin = nguoi.LaAdmin,
            CoTheSua = bc.TrangThai == TrangThaiBaoCaoEnum.MoiBao,
            CoTheHuy = CoTheHuy(bc, lienQuan, nguoi),
            CoTheXoa = CoTheXoa(bc, lienQuan),
        };
    }

    public async Task<ServiceResult<BaoCaoSuCoFormViewModel>> LayDeSuaAsync(int id, NguoiThaoTac nguoi)
    {
        var bc = await _repo.GetByIdAsync(id);
        if (bc is null || !DuocXem(bc, nguoi))
            return ServiceResult<BaoCaoSuCoFormViewModel>.Fail(KhongTimThay);

        if (bc.TrangThai != TrangThaiBaoCaoEnum.MoiBao)
            return ServiceResult<BaoCaoSuCoFormViewModel>.Fail(
                "Chỉ sửa được báo cáo đang ở trạng thái Mới báo (chưa được tiếp nhận)."
            );

        return ServiceResult<BaoCaoSuCoFormViewModel>.Ok(
            new BaoCaoSuCoFormViewModel
            {
                MaBaoCao = bc.MaBaoCao,
                MaThietBi = bc.MaThietBi,
                MoTaSuCo = bc.MoTaSuCo,
                MucDoSuCo = bc.MucDoSuCo,
                GhiChu = bc.GhiChu,
            }
        );
    }

    public async Task<List<(int Ma, string Ten)>> LayThietBiChoChonAsync(NguoiThaoTac nguoi, int? giuLaiMa)
    {
        // Admin thấy mọi thiết bị; người dùng chỉ thấy thiết bị thuộc khu vực của mình
        if (nguoi.LaAdmin)
            return await _repo.LayThietBiChoChonAsync(false, null, giuLaiMa);

        var tk = await _repo.LayTaiKhoanAsync(nguoi.MaTaiKhoan);
        return await _repo.LayThietBiChoChonAsync(true, tk?.MaKhuVuc, giuLaiMa);
    }

    public Task<int?> LayMaTaiKhoanXemThuAsync() => _repo.LayMaTaiKhoanNguoiDungDauTienAsync();

    // ---------------- Gửi báo cáo ----------------
    public async Task<ServiceResult<int>> GuiBaoCaoAsync(BaoCaoSuCoFormViewModel vm, NguoiThaoTac nguoi)
    {
        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, nguoi, laThemMoi: true);
        if (!loi.Succeeded)
            return Chuyen(loi);

        var bc = new BaoCaoSuCo
        {
            MaThietBi = vm.MaThietBi!.Value,
            MaTaiKhoanBaoCao = nguoi.MaTaiKhoan,
            NgayBaoCao = DateTime.Now, // hệ thống tự gán, không nhận từ form
            MoTaSuCo = vm.MoTaSuCo,
            MucDoSuCo = vm.MucDoSuCo!.Value,
            TrangThai = TrangThaiBaoCaoEnum.MoiBao,
            GhiChu = vm.GhiChu,
        };

        await _repo.AddAsync(bc);
        await _repo.SaveChangesAsync();
        return ServiceResult<int>.Ok(bc.MaBaoCao);
    }

    // ---------------- Sửa ----------------
    public async Task<ServiceResult> SuaAsync(BaoCaoSuCoFormViewModel vm, NguoiThaoTac nguoi)
    {
        var bc = await _repo.GetByIdAsync(vm.MaBaoCao);
        if (bc is null || !DuocXem(bc, nguoi))
            return ServiceResult.Fail(KhongTimThay);

        if (bc.TrangThai != TrangThaiBaoCaoEnum.MoiBao)
            return ServiceResult.Fail(
                "Chỉ sửa được báo cáo đang ở trạng thái Mới báo (chưa được tiếp nhận)."
            );

        ChuanHoa(vm);

        var loi = await KiemTraAsync(vm, nguoi, laThemMoi: false);
        if (!loi.Succeeded)
            return loi;

        // Không đổi thiết bị, người báo, ngày báo cáo, trạng thái
        bc.MoTaSuCo = vm.MoTaSuCo;
        bc.MucDoSuCo = vm.MucDoSuCo!.Value;
        bc.GhiChu = vm.GhiChu;

        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Hủy ----------------
    public async Task<ServiceResult> HuyAsync(int id, NguoiThaoTac nguoi)
    {
        var bc = await _repo.GetByIdAsync(id);
        if (bc is null || !DuocXem(bc, nguoi))
            return ServiceResult.Fail(KhongTimThay);

        var lienQuan = await _repo.DemDuLieuLienQuanAsync(id);
        if (!CoTheHuy(bc, lienQuan, nguoi))
            return ServiceResult.Fail(
                nguoi.LaAdmin
                    ? "Chỉ hủy được báo cáo Mới báo/Đã tiếp nhận và chưa có phiếu xử lý đang hoạt động."
                    : "Chỉ hủy được báo cáo đang ở trạng thái Mới báo."
            );

        bc.TrangThai = TrangThaiBaoCaoEnum.DaHuy;
        await _repo.SaveChangesAsync();
        return ServiceResult.Ok();
    }

    // ---------------- Xóa an toàn ----------------
    public async Task<ServiceResult> XoaAsync(int id, NguoiThaoTac nguoi)
    {
        var bc = await _repo.GetByIdAsync(id);
        if (bc is null || !DuocXem(bc, nguoi))
            return ServiceResult.Fail(KhongTimThay);

        var lienQuan = await _repo.DemDuLieuLienQuanAsync(id);
        if (!CoTheXoa(bc, lienQuan))
            return ServiceResult.Fail(
                "Không thể xóa báo cáo đã được tiếp nhận hoặc đã phát sinh phiếu xử lý/tệp đính kèm. Hãy dùng chức năng Hủy báo cáo để giữ lịch sử."
            );

        try
        {
            _repo.Remove(bc);
            await _repo.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return ServiceResult.Fail("Không thể xóa: báo cáo còn dữ liệu liên quan. Hãy hủy báo cáo.");
        }
        return ServiceResult.Ok();
    }

    // ---------------- Quy tắc dùng chung ----------------

    /// Admin xem mọi báo cáo; người khác chỉ xem báo cáo do chính mình gửi.
    private static bool DuocXem(BaoCaoSuCo bc, NguoiThaoTac nguoi) =>
        nguoi.LaAdmin || bc.MaTaiKhoanBaoCao == nguoi.MaTaiKhoan;

    private static bool CoTheHuy(BaoCaoSuCo bc, DuLieuLienQuanBaoCao lienQuan, NguoiThaoTac nguoi)
    {
        if (lienQuan.SoPhieuConHieuLuc > 0)
            return false;
        return nguoi.LaAdmin
            ? bc.TrangThai is TrangThaiBaoCaoEnum.MoiBao or TrangThaiBaoCaoEnum.DaTiepNhan
            : bc.TrangThai == TrangThaiBaoCaoEnum.MoiBao;
    }

    private static bool CoTheXoa(BaoCaoSuCo bc, DuLieuLienQuanBaoCao lienQuan) =>
        bc.TrangThai == TrangThaiBaoCaoEnum.MoiBao && !lienQuan.CoDuLieu;

    private static void ChuanHoa(BaoCaoSuCoFormViewModel vm)
    {
        vm.MoTaSuCo = (vm.MoTaSuCo ?? string.Empty).Trim();
        vm.GhiChu = string.IsNullOrWhiteSpace(vm.GhiChu) ? null : vm.GhiChu.Trim();
    }

    private static ServiceResult<int> Chuyen(ServiceResult loi)
    {
        var r = new ServiceResult<int>();
        foreach (var e in loi.Errors)
            r.AddError(e.Message, e.Field);
        return r;
    }

    private async Task<ServiceResult> KiemTraAsync(
        BaoCaoSuCoFormViewModel vm,
        NguoiThaoTac nguoi,
        bool laThemMoi
    )
    {
        var kq = ServiceResult.Ok();

        if (vm.MoTaSuCo.Length == 0)
            kq.AddError("Vui lòng mô tả sự cố", nameof(BaoCaoSuCoFormViewModel.MoTaSuCo));
        else if (vm.MoTaSuCo.Length > 500)
            kq.AddError("Mô tả sự cố tối đa 500 ký tự", nameof(BaoCaoSuCoFormViewModel.MoTaSuCo));

        if (vm.MucDoSuCo is not { } mucDo || !Enum.IsDefined(mucDo))
            kq.AddError("Mức độ sự cố không hợp lệ", nameof(BaoCaoSuCoFormViewModel.MucDoSuCo));

        // Thiết bị chỉ kiểm tra khi tạo mới (sửa thì thiết bị bị khóa, không đổi)
        if (laThemMoi)
        {
            if (vm.MaThietBi is not { } maTb)
            {
                kq.AddError("Vui lòng chọn thiết bị", nameof(BaoCaoSuCoFormViewModel.MaThietBi));
            }
            else
            {
                var tb = await _repo.LayThietBiAsync(maTb);
                if (tb is null)
                    kq.AddError("Thiết bị không tồn tại", nameof(BaoCaoSuCoFormViewModel.MaThietBi));
                else if (tb.TinhTrang == TinhTrangThietBiEnum.NgungSuDung)
                    kq.AddError(
                        "Thiết bị đã ngừng sử dụng, không thể báo cáo sự cố",
                        nameof(BaoCaoSuCoFormViewModel.MaThietBi)
                    );
                else if (!nguoi.LaAdmin)
                {
                    var tk = await _repo.LayTaiKhoanAsync(nguoi.MaTaiKhoan);
                    if (tk?.MaKhuVuc is null || tk.MaKhuVuc != tb.MaKhuVuc)
                        kq.AddError(
                            "Thiết bị không thuộc khu vực của bạn",
                            nameof(BaoCaoSuCoFormViewModel.MaThietBi)
                        );
                }
            }

            // Người báo phải là tài khoản tồn tại và đang hoạt động
            var nguoiBao = await _repo.LayTaiKhoanAsync(nguoi.MaTaiKhoan);
            if (nguoiBao is null || !nguoiBao.TrangThai)
                kq.AddError("Tài khoản gửi báo cáo không hợp lệ hoặc đã bị khóa");
        }

        return kq;
    }
}
