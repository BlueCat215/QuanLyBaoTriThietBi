// Nội dung: Module 2 - Điều kiện Tìm kiếm + Lọc + Sắp xếp danh sách Thiết bị
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

/// Phân loại hạn bảo trì
public enum TrangThaiHanBaoTriEnum
{
    ChuaDenHan,
    SapDenHan,
    QuaHan,
}

public enum ThietBiSapXep
{
    TenAZ,
    TenZA,
    NgaySuDungMoiNhat,
    NgaySuDungCuNhat,
    BaoTriSomNhat,
    BaoTriMuonNhat,
    ChuKyNganNhat,
    ChuKyDaiNhat,
}

public class ThietBiBoLoc
{
    public string? TuKhoa { get; set; }
    public int? MaLoaiThietBi { get; set; }
    public int? MaKhuVuc { get; set; }
    public TinhTrangThietBiEnum? TinhTrang { get; set; }
    public TrangThaiHanBaoTriEnum? HanBaoTri { get; set; }
    public DateTime? NgaySuDungTu { get; set; }
    public DateTime? NgaySuDungDen { get; set; }
    public ThietBiSapXep SapXep { get; set; } = ThietBiSapXep.TenAZ;

    /// Có điều kiện LỌC nào đang bật không (không tính Sắp xếp).
    public bool CoBoLoc =>
        !string.IsNullOrWhiteSpace(TuKhoa)
        || MaLoaiThietBi.HasValue
        || MaKhuVuc.HasValue
        || TinhTrang.HasValue
        || HanBaoTri.HasValue
        || NgaySuDungTu.HasValue
        || NgaySuDungDen.HasValue;

    /// "Từ ngày" lớn hơn "Đến ngày": khoảng ngày bị bỏ qua và trang hiện cảnh báo.
    public bool KhoangNgayKhongHopLe =>
        NgaySuDungTu.HasValue
        && NgaySuDungDen.HasValue
        && NgaySuDungTu.Value.Date > NgaySuDungDen.Value.Date;
}

public static class ThietBiBoLocText
{
    public static string SapXep(ThietBiSapXep v) =>
        v switch
        {
            ThietBiSapXep.TenAZ => "Tên A → Z",
            ThietBiSapXep.TenZA => "Tên Z → A",
            ThietBiSapXep.NgaySuDungMoiNhat => "Ngày đưa vào sử dụng: mới nhất",
            ThietBiSapXep.NgaySuDungCuNhat => "Ngày đưa vào sử dụng: cũ nhất",
            ThietBiSapXep.BaoTriSomNhat => "Bảo trì tiếp theo: sớm nhất",
            ThietBiSapXep.BaoTriMuonNhat => "Bảo trì tiếp theo: muộn nhất",
            ThietBiSapXep.ChuKyNganNhat => "Chu kỳ bảo trì: ngắn → dài",
            ThietBiSapXep.ChuKyDaiNhat => "Chu kỳ bảo trì: dài → ngắn",
            _ => v.ToString(),
        };

    public static string HanBaoTri(TrangThaiHanBaoTriEnum v, int ngayCanhBao) =>
        v switch
        {
            TrangThaiHanBaoTriEnum.ChuaDenHan => "Chưa đến hạn",
            TrangThaiHanBaoTriEnum.SapDenHan => $"Sắp đến hạn (trong {ngayCanhBao} ngày)",
            TrangThaiHanBaoTriEnum.QuaHan => "Quá hạn",
            _ => v.ToString(),
        };
}
