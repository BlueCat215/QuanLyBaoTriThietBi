// Họ tên: Nguyễn Khắc Hiếu - Lớp: TI17A4HN - Nhóm: UNETI01
// Module 5: Báo cáo Thống kê đa chiều - IThongKeRepository.cs
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
