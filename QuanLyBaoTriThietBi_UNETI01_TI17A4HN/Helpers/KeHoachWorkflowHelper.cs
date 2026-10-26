// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 -Kiểm tra khóa (IsLocked):-Trả về true nếu trạng thái là Hoàn thành hoặc Đã hủy
//                                              -Ép luồng (CanTransitionTo): Quy định các bước chuyển hợp lệ (Ví dụ: Chờ thực hiện chỉ được sang Đã phân công hoặc Hủy, không được nhảy cóc sang Hoàn thành).
//                                              -Hiển thị UI: Quy đổi mã sang tên Tiếng Việt và màu nhãn (Badge).
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;
namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers
{
    public class KeHoachWorkflowHelper
    {
        public static class KeHoachWorkflowHelper
    {
        // 1. Kiểm tra trạng thái kết thúc (Khóa chặn sửa)
        public static bool IsLocked(TrangThaiKeHoach status)
        {
            return status == TrangThaiKeHoach.HoanThanh || status == TrangThaiKeHoach.DaHuy;
        }

        // 2. Kiểm tra điều kiện chuyển trạng thái đúng luồng
        public static bool CanTransitionTo(TrangThaiKeHoach current, TrangThaiKeHoach target)
        {
            if (current == target) return true;
            if (IsLocked(current)) return false; // Đã Hoàn thành hoặc Hủy thì khóa 100%

            return current switch
            {
                TrangThaiKeHoach.ChoThucHien => target == TrangThaiKeHoach.DaPhanCong 
                                             || target == TrangThaiKeHoach.DaHuy,

                TrangThaiKeHoach.DaPhanCong => target == TrangThaiKeHoach.DangThucHien 
                                            || target == TrangThaiKeHoach.ChoThucHien 
                                            || target == TrangThaiKeHoach.DaHuy,

                TrangThaiKeHoach.DangThucHien => target == TrangThaiKeHoach.ChoNghiemThu 
                                             || target == TrangThaiKeHoach.DaHuy,

                TrangThaiKeHoach.ChoNghiemThu => target == TrangThaiKeHoach.HoanThanh 
                                              || target == TrangThaiKeHoach.DangThucHien // Trả về nếu nghiệm thu chưa đạt
                                              || target == TrangThaiKeHoach.DaHuy,

                _ => false
            };
        }

        // 3. Lấy tên tiếng Việt hiển thị
        public static string GetDisplayName(TrangThaiKeHoach status) => status switch
        {
            TrangThaiKeHoach.ChoThucHien => "Chờ thực hiện",
            TrangThaiKeHoach.DaPhanCong => "Đã phân công",
            TrangThaiKeHoach.DangThucHien => "Đang thực hiện",
            TrangThaiKeHoach.ChoNghiemThu => "Chờ nghiệm thu",
            TrangThaiKeHoach.HoanThanh => "Hoàn thành",
            TrangThaiKeHoach.DaHuy => "Đã hủy",
            _ => "Không xác định"
        };

        // 4. Lấy màu badge Bootstrap
        public static string GetBadgeClass(TrangThaiKeHoach status) => status switch
        {
            TrangThaiKeHoach.ChoThucHien => "bg-warning text-dark",
            TrangThaiKeHoach.DaPhanCong => "bg-info text-white",
            TrangThaiKeHoach.DangThucHien => "bg-primary",
            TrangThaiKeHoach.ChoNghiemThu => "bg-warning text-white",
            TrangThaiKeHoach.HoanThanh => "bg-success",
            TrangThaiKeHoach.DaHuy => "bg-secondary",
            _ => "bg-light text-dark"
        };
    }
    }
}