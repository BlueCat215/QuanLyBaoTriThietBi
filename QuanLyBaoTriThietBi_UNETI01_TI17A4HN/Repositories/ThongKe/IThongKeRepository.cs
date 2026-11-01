// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Repository Báo cáo thống kê
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IThongKeRepository
{
    Task<List<ThietBi>> LayDanhSachThietBiAsync();
    Task<List<LoaiThietBi>> LayDanhSachLoaiThietBiAsync();
    Task<List<KhuVuc>> LayDanhSachKhuVucAsync();
    Task<List<KyThuatVien>> LayDanhSachKyThuatVienAsync();
    Task<List<BaoCaoSuCo>> LayBaoCaoSuCoTheoKhoangNgayAsync(DateTime startDate, DateTime endDate);
    Task<List<KeHoachBaoTri>> LayKeHoachBaoTriTheoKhoangNgayAsync(DateTime startDate, DateTime endDate);
    Task<List<PhieuXuLy>> LayPhieuXuLyTheoKhoangNgayAsync(DateTime startDate, DateTime endDate);
    Task<List<ChiTietXuLy>> LayChiTietXuLyTrongNamAsync(int nam);
}
