// Họ tên: Nguyễn Minh Đức
// Nội dung: Module 2 - LINQ Lọc + Sắp xếp Thiết bị, tính hạn bảo trì và phân loại thống nhất toàn hệ thống (Việc 15, 27, 28, 29)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

/// Mở rộng LINQ cho Thiết bị:
/// - Ngày bảo trì tiếp theo = (NgayBaoTriGanNhat ?? NgayDuaVaoSuDung) + ChuKyBaoTriThang (tháng).
/// - Phân loại hạn bảo trì: Quá hạn, Sắp đến hạn (ngưỡng 7 ngày), Chưa đến hạn.
/// - Áp dụng thống nhất cho Module 2, Dashboard (Module 5) và Báo cáo Thống kê.
public static class ThietBiQueryExtensions
{
    private const int SoTuKhoaToiDa = 5;

    /// Áp dụng toàn bộ điều kiện Tìm kiếm + Lọc (chưa Sắp xếp).
    public static IQueryable<ThietBi> ApDungLoc(
        this IQueryable<ThietBi> query,
        ThietBiBoLoc boLoc,
        DateTime homNay
    )
    {
        // Tìm kiếm: mỗi từ khớp Tên HOẶC Serial HOẶC Tên khu vực (AND giữa các từ, OR giữa các cột)
        if (!string.IsNullOrWhiteSpace(boLoc.TuKhoa))
        {
            var cacTu = boLoc
                .TuKhoa.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
                .Take(SoTuKhoaToiDa);

            foreach (var tu in cacTu)
            {
                query = query.Where(t =>
                    t.TenThietBi.Contains(tu)
                    || t.SoSerial.Contains(tu)
                    || t.KhuVuc!.TenKhuVuc.Contains(tu)
                );
            }
        }

        // Lọc theo loại / khu vực / tình trạng
        if (boLoc.MaLoaiThietBi is { } maLoai)
            query = query.Where(t => t.MaLoaiThietBi == maLoai);

        if (boLoc.MaKhuVuc is { } maKhuVuc)
            query = query.Where(t => t.MaKhuVuc == maKhuVuc);

        if (boLoc.TinhTrang is { } tinhTrang)
            query = query.Where(t => t.TinhTrang == tinhTrang);

        // Lọc theo khoảng ngày đưa vào sử dụng (gồm cả hai đầu mút)
        if (!boLoc.KhoangNgayKhongHopLe)
        {
            if (boLoc.NgaySuDungTu is { } tu)
            {
                var tuNgay = tu.Date;
                query = query.Where(t => t.NgayDuaVaoSuDung >= tuNgay);
            }
            if (boLoc.NgaySuDungDen is { } den)
            {
                var denNgayKeTiep = den.Date.AddDays(1);
                query = query.Where(t => t.NgayDuaVaoSuDung < denNgayKeTiep);
            }
        }

        if (boLoc.HanBaoTri is { } han)
            query = query.LocTheoHanBaoTri(han, homNay);

        return query;
    }

    /// Lọc theo hạn bảo trì, cùng tiêu chí với UiHelper.HanBaoTri:
    /// Quá hạn = trước hôm nay; Sắp đến hạn = từ hôm nay đến hết NgayCanhBaoSapDenHan ngày; còn lại = Chưa đến hạn.
    /// Thiết bị Ngừng sử dụng không tính hạn (giống cột "Bảo trì tiếp theo" trên giao diện).
    public static IQueryable<ThietBi> LocTheoHanBaoTri(
        this IQueryable<ThietBi> query,
        TrangThaiHanBaoTriEnum han,
        DateTime homNay
    )
    {
        var ngayHomNay = homNay.Date;
        // Mốc đầu tiên của nhóm "Chưa đến hạn" (so sánh < để đúng cả khi cột ngày có kèm giờ)
        var mocChuaDenHan = ngayHomNay.AddDays(UiHelper.NgayCanhBaoSapDenHan + 1);

        query = query.Where(t => t.TinhTrang != TinhTrangThietBiEnum.NgungSuDung);

        return han switch
        {
            TrangThaiHanBaoTriEnum.QuaHan => query.Where(t =>
                (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                < ngayHomNay
            ),

            TrangThaiHanBaoTriEnum.SapDenHan => query.Where(t =>
                (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                    >= ngayHomNay
                && (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                    < mocChuaDenHan
            ),

            _ => query.Where(t =>
                (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                >= mocChuaDenHan
            ),
        };
    }

    /// Lọc thiết bị sắp đến hạn bảo trì (trong vòng UiHelper.NgayCanhBaoSapDenHan ngày).
    /// Dùng thống nhất cho Module 2, Dashboard (Module 5) và Thống kê.
    public static IQueryable<ThietBi> SapDenHanBaoTri(
        this IQueryable<ThietBi> query,
        DateTime? homNay = null
    ) => query.LocTheoHanBaoTri(TrangThaiHanBaoTriEnum.SapDenHan, homNay ?? DateTime.Today);

    /// Lọc thiết bị đã quá hạn bảo trì.
    /// Dùng thống nhất cho Module 2, Dashboard (Module 5) và Thống kê.
    public static IQueryable<ThietBi> QuaHanBaoTri(
        this IQueryable<ThietBi> query,
        DateTime? homNay = null
    ) => query.LocTheoHanBaoTri(TrangThaiHanBaoTriEnum.QuaHan, homNay ?? DateTime.Today);

    /// Lọc thiết bị chưa đến hạn bảo trì.
    /// Dùng thống nhất cho Module 2, Dashboard (Module 5) và Thống kê.
    public static IQueryable<ThietBi> ChuaDenHanBaoTri(
        this IQueryable<ThietBi> query,
        DateTime? homNay = null
    ) => query.LocTheoHanBaoTri(TrangThaiHanBaoTriEnum.ChuaDenHan, homNay ?? DateTime.Today);

    /// Luôn kèm khóa phụ (tên, mã) để thứ tự ổn định.
    public static IQueryable<ThietBi> ApDungSapXep(
        this IQueryable<ThietBi> query,
        ThietBiSapXep sapXep
    ) =>
        sapXep switch
        {
            ThietBiSapXep.TenZA => query
                .OrderByDescending(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.NgaySuDungMoiNhat => query
                .OrderByDescending(t => t.NgayDuaVaoSuDung)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.NgaySuDungCuNhat => query
                .OrderBy(t => t.NgayDuaVaoSuDung)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.BaoTriSomNhat => query
                .OrderBy(t =>
                    (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                )
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.BaoTriMuonNhat => query
                .OrderByDescending(t =>
                    (t.NgayBaoTriGanNhat ?? t.NgayDuaVaoSuDung).AddMonths(t.ChuKyBaoTriThang)
                )
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.ChuKyNganNhat => query
                .OrderBy(t => t.ChuKyBaoTriThang)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            ThietBiSapXep.ChuKyDaiNhat => query
                .OrderByDescending(t => t.ChuKyBaoTriThang)
                .ThenBy(t => t.TenThietBi)
                .ThenBy(t => t.MaThietBi),

            // TenAZ (mặc định)
            _ => query.OrderBy(t => t.TenThietBi).ThenBy(t => t.MaThietBi),
        };
}
