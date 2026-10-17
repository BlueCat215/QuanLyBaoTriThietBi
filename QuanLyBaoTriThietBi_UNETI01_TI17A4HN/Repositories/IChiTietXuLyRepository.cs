// Họ tên: Nguyễn Khắc Hiếu
// MSSV: <23103100225>
// Nội dung: Module 5 - Interface Repository Chi tiết xử lý
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories;

public interface IChiTietXuLyRepository : IRepository<ChiTietXuLy>
{
    /// Danh sách phiếu (kèm số dòng chi tiết và tổng chi phí) để chọn phiếu cần xem/nhập chi tiết.
    Task<List<PhieuXuLyRow>> LayDanhSachPhieuAsync(TrangThaiPhieuXuLyEnum? trangThai);

    /// Tóm tắt 1 phiếu (loại, trạng thái, nguồn, thiết bị). Null nếu không tồn tại.
    Task<PhieuTomTat?> LayPhieuAsync(int maPhieuXuLy);

    /// Các dòng chi tiết của 1 phiếu, cũ -> mới.
    Task<List<ChiTietXuLyRow>> LayTheoPhieuAsync(int maPhieuXuLy);

    /// TongChiPhi của phiếu = SUM(ThanhTien), tính trong DB.
    Task<decimal> TongChiPhiAsync(int maPhieuXuLy);

    Task<VatTu?> LayVatTuAsync(int maVatTu);

    /// Vật tư đang sử dụng (giữ lại vật tư hiện tại của dòng đang sửa dù đã ngừng sử dụng).
    Task<List<VatTuLuaChon>> LayVatTuChoChonAsync(int? giuLaiMa);
}
