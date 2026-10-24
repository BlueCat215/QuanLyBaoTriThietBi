// Nội dung: Module 2 - Điều kiện Tìm kiếm + Lọc + Sắp xếp + Phân trang danh sách Thiết bị
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

    // Phân trang
    public int Trang { get; set; } = 1;
    public int SoBanGhiMoiTrang { get; set; } = 10;

    /// Có điều kiện LỌC nào đang bật không (không tính Sắp xếp và Phân trang).
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

    /// Tạo từ điển route values để gắn vào link phân trang, giữ nguyên các điều kiện tìm kiếm, lọc, sắp xếp
    public Dictionary<string, string> ToRouteValues(int? trang = null)
    {
        var route = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(TuKhoa))
            route[nameof(TuKhoa)] = TuKhoa;

        if (MaLoaiThietBi.HasValue)
            route[nameof(MaLoaiThietBi)] = MaLoaiThietBi.Value.ToString();

        if (MaKhuVuc.HasValue)
            route[nameof(MaKhuVuc)] = MaKhuVuc.Value.ToString();

        if (TinhTrang.HasValue)
            route[nameof(TinhTrang)] = TinhTrang.Value.ToString();

        if (HanBaoTri.HasValue)
            route[nameof(HanBaoTri)] = HanBaoTri.Value.ToString();

        if (NgaySuDungTu.HasValue)
            route[nameof(NgaySuDungTu)] = NgaySuDungTu.Value.ToString("yyyy-MM-dd");

        if (NgaySuDungDen.HasValue)
            route[nameof(NgaySuDungDen)] = NgaySuDungDen.Value.ToString("yyyy-MM-dd");

        if (SapXep != ThietBiSapXep.TenAZ)
            route[nameof(SapXep)] = SapXep.ToString();

        if (SoBanGhiMoiTrang != 10)
            route[nameof(SoBanGhiMoiTrang)] = SoBanGhiMoiTrang.ToString();

        var p = trang ?? Trang;
        if (p > 1)
            route[nameof(Trang)] = p.ToString();

        return route;
    }
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
