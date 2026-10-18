// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Service Dashboard (TẠM: trả dữ liệu minh họa để dựng khung giao diện, việc 21)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

/// TODO tuần 4 (việc 44): thay thân LayThongKeAsync bằng truy vấn LINQ thật (xem ghi chú từng chỉ số bên dưới),
/// đặt LaDuLieuMinhHoa = false. View, Controller và ViewModel không cần sửa.
public class DashboardService : IDashboardService
{
    public Task<DashboardViewModel> LayThongKeAsync()
    {
        var homNay = DateTime.Today;

        // Dữ liệu TẠM. Khi nối số liệu thật chỉ cần thay các Dictionary này bằng kết quả GroupBy/Count.
        var tinhTrang = new Dictionary<TinhTrangThietBiEnum, int>
        {
            [TinhTrangThietBiEnum.DangHoatDong] = 24,
            [TinhTrangThietBiEnum.DangBaoTri] = 3,
            [TinhTrangThietBiEnum.TamNgungDoSuCo] = 2,
            [TinhTrangThietBiEnum.NgungSuDung] = 1,
        };
        var suCo = new Dictionary<TrangThaiBaoCaoEnum, int>
        {
            [TrangThaiBaoCaoEnum.MoiBao] = 3,
            [TrangThaiBaoCaoEnum.DaTiepNhan] = 2,
            [TrangThaiBaoCaoEnum.DangXuLy] = 4,
            [TrangThaiBaoCaoEnum.ChoNghiemThu] = 2,
            [TrangThaiBaoCaoEnum.HoanThanh] = 14,
            [TrangThaiBaoCaoEnum.DaHuy] = 1,
        };
        var phieu = new Dictionary<TrangThaiPhieuXuLyEnum, int>
        {
            [TrangThaiPhieuXuLyEnum.ChoPhanCong] = 2,
            [TrangThaiPhieuXuLyEnum.DaPhanCong] = 3,
            [TrangThaiPhieuXuLyEnum.DangXuLy] = 4,
            [TrangThaiPhieuXuLyEnum.ChoNghiemThu] = 2,
            [TrangThaiPhieuXuLyEnum.HoanThanh] = 13,
            [TrangThaiPhieuXuLyEnum.DaHuy] = 1,
        };

        var vm = new DashboardViewModel
        {
            LaDuLieuMinhHoa = true,

            // 1+2: Count() và GroupBy(TinhTrang) trên ThietBis
            ThietBiTheoTinhTrang = TheoEnum<TinhTrangThietBiEnum>(tinhTrang, UiHelper.Badge),
            TongThietBi = tinhTrang.Values.Sum(),

            // 3+4: dùng ĐÚNG logic hạn bảo trì của SV2 (việc 29, NgayBaoTriTiepTheo + UiHelper.NgayCanhBaoSapDenHan);
            // NgayBaoTriTiepTheo là [NotMapped] nên không dùng được trong Where của EF - cần biểu thức SQL dùng chung.
            ThietBiSapDenHan = 4,
            ThietBiQuaHan = 3,

            // 5: Count(KeHoachBaoTris, TrangThai == ChoThucHien)
            KeHoachChoThucHien = 6,

            // 6: GroupBy(TrangThai) trên BaoCaoSuCos
            SuCoTheoTrangThai = TheoEnum<TrangThaiBaoCaoEnum>(suCo, UiHelper.Badge),

            // 7: GroupBy(TrangThai) trên PhieuXuLys
            PhieuTheoTrangThai = TheoEnum<TrangThaiPhieuXuLyEnum>(phieu, UiHelper.Badge),

            // 8: SUM(ChiTietXuLy.ThanhTien) trong tháng - cần chốt mốc tháng (NgayCapNhat của dòng hay NgayHoanThanh của phiếu)
            TongChiPhiThang = 12_450_000m,
            Thang = homNay.Month,
            Nam = homNay.Year,
        };

        return Task.FromResult(vm);
    }

    /// Chuyển Dictionary enum -> danh sách mục theo ĐÚNG thứ tự khai báo enum; enum thiếu số liệu = 0.
    private static List<ThongKeMuc> TheoEnum<TEnum>(
        IReadOnlyDictionary<TEnum, int> soLieu,
        Func<TEnum, BadgeInfo> badge
    )
        where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(v =>
            {
                var b = badge(v);
                return new ThongKeMuc(b.Text, soLieu.GetValueOrDefault(v), UiHelper.MauCot(b));
            })
            .ToList();
}
