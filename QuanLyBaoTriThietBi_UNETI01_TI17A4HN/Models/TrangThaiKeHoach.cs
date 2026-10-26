/// 2313100207 Pham Viet Hoan: Module 3: Tạo Model 3 Tạo Enum trạng thái TrangThaiKeHoach.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums
{
    public enum TrangThaiKeHoach
    {
        ChoThucHien = 0,    // Chờ thực hiện
        DaPhanCong = 1,     // Đã phân công
        DangThucHien = 2,   // Đang thực hiện
        ChoNghiemThu = 3,   // Chờ nghiệm thu
        HoanThanh = 4,      // Hoàn thành (Bị khóa)
        DaHuy = 5           // Đã hủy (Bị khóa)
    }
}