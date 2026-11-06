// Họ tên: Phạm Việt Hoàn - Nguyễn Khắc Hiếu
// MSSV: 23103100207 - 23103100225
// Nội dung: Module 3 & 5 - STT 59: ViewModel Hệ thống thông báo nội bộ (In-App Notification Bell)
#nullable enable
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.ViewModels;

public class ThongBaoItemDto
{
    public int MaThongBao { get; set; }
    public string NoiDung { get; set; } = string.Empty;
    public DateTime NgayTao { get; set; }
    public bool DaDoc { get; set; }
    public int? MaThietBi { get; set; }
    public string? TenThietBi { get; set; }
    public string? SoSerial { get; set; }

    /// Hiển thị thời gian thân thiện (Ví dụ: 5 phút trước, 2 giờ trước, Hôm qua, 15/10/2026)
    public string ThoiGianAgo
    {
        get
        {
            var span = DateTime.Now - NgayTao;
            if (span.TotalMinutes < 1) return "Vừa xong";
            if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} phút trước";
            if (span.TotalHours < 24) return $"{(int)span.TotalHours} giờ trước";
            if (span.TotalDays < 2) return $"Hôm qua {NgayTao:HH:mm}";
            if (span.TotalDays < 7) return $"{(int)span.TotalDays} ngày trước";
            return NgayTao.ToString("dd/MM/yyyy HH:mm");
        }
    }

    /// Tự động gán biểu tượng theo ngữ cảnh nội dung thông báo
    public string Icon
    {
        get
        {
            var lower = NoiDung.ToLower();
            if (lower.Contains("sự cố") || lower.Contains("hỏng")) return "warning";
            if (lower.Contains("phân công") || lower.Contains("giao việc")) return "assignment_ind";
            if (lower.Contains("hoàn thành") || lower.Contains("nghiệm thu")) return "task_alt";
            if (lower.Contains("bảo trì") || lower.Contains("kế hoạch") || lower.Contains("quá hạn")) return "calendar_clock";
            return "notifications";
        }
    }

    public string BadgeClass
    {
        get
        {
            var lower = NoiDung.ToLower();
            if (lower.Contains("sự cố") || lower.Contains("hỏng")) return "text-danger bg-danger-subtle";
            if (lower.Contains("phân công")) return "text-primary bg-primary-subtle";
            if (lower.Contains("hoàn thành") || lower.Contains("nghiệm thu")) return "text-success bg-success-subtle";
            if (lower.Contains("quá hạn")) return "text-warning bg-warning-subtle";
            return "text-info bg-info-subtle";
        }
    }

    /// Đường dẫn liên kết trực tiếp nếu thông báo gắn với thực thể
    public string? UrlDieuHuong
    {
        get
        {
            if (MaThietBi.HasValue) return $"/ThietBi/Details/{MaThietBi.Value}";
            var lower = NoiDung.ToLower();
            if (lower.Contains("sự cố")) return "/BaoCaoSuCo/Index";
            if (lower.Contains("phân công") || lower.Contains("phiếu")) return "/PhieuXuLy/Index";
            if (lower.Contains("kế hoạch")) return "/KeHoachBaoTri/Index";
            return null;
        }
    }
}

/// Dữ liệu trả về cho AJAX Popup chuông thông báo trên Header
public class ThongBaoNhanhViewModel
{
    public int SoChuaDoc { get; set; }
    public List<ThongBaoItemDto> DanhSach { get; set; } = new();
}

/// Dữ liệu cho trang danh sách toàn bộ thông báo (/ThongBao/Index)
public class ThongBaoIndexViewModel
{
    public PagedResult<ThongBaoItemDto> PagedItems { get; set; } = new();
    public List<ThongBaoItemDto> Items => PagedItems.Items;
    public bool? ChiChuaDoc { get; set; }
    public int SoChuaDoc { get; set; }
    public int TongSo { get; set; }
}
