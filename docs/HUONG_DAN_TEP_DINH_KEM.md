# HƯỚNG DẪN TÍCH HỢP HỆ THỐNG TỆP ĐÍNH KÈM ĐA HÌNH (STT 53)

> **Nhóm thực hiện:** UNETI01 - Lớp TI17A4HN  
> **Người xây dựng hạ tầng (Module 2):** Nguyễn Minh Đức  
> **Người phối hợp sử dụng (Module 4):** Trịnh Trung Hoàng (STT 55 - Đính kèm ảnh hiện trường sự cố)

---

## 1. Nguyên tắc thiết kế (Theo Mục 6.2 THIETKE.md)
- Bảng `TepDinhKem` dùng chung cho toàn bộ hệ thống bằng cơ chế khóa ngoại đa hình:
  - `LoaiDoiTuong` (Enum: `ThietBi`, `BaoCaoSuCo`, `ChiTietXuLy`)
  - `MaDoiTuong` (ID đối tượng cha tương ứng)
- Do là quan hệ đa hình, SQL Server không có Foreign Key vật lý. Tầng Service (`ITepDinhKemService`) đã tự động kiểm tra sự tồn tại của đối tượng cha trước khi lưu.
- File vật lý được lưu trữ riêng theo từng thư mục con:
  - Thiết bị: `wwwroot/uploads/thietbi/`
  - Báo cáo sự cố: `wwwroot/uploads/baocaosuco/`
  - Chi tiết xử lý: `wwwroot/uploads/chitietxuly/`
- Tên file lưu trên đĩa được sinh ngẫu nhiên bằng `Guid:N` + đuôi mở rộng để chống ghi đè và chống lộ cấu trúc thư mục.
- Giới hạn kích thước file: $\le 5\,\text{MB}$.
- Chặn tuyệt đối các tệp nguy hại (`.exe`, `.bat`, `.sh`, `.php`, `.asp`, `.js`,...).

---

## 2. Hướng dẫn dành cho Module 4 (Trịnh Trung Hoàng - STT 55)

Khi pull code về triển khai tính năng **STT 55 (Đính kèm ảnh hiện trường sự cố)**, bạn **không cần viết lại** Repository hay Service xử lý file, chỉ cần gọi `ITepDinhKemService` theo các bước sau:

### Bước 1: Inject Service vào Controller / Service của Module 4
```csharp
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

public class BaoCaoSuCoController : Controller
{
    private readonly IBaoCaoSuCoService _service;
    private readonly ITepDinhKemService _tepDinhKemService; // Inject service có sẵn

    public BaoCaoSuCoController(
        IBaoCaoSuCoService service,
        ITepDinhKemService tepDinhKemService
    )
    {
        _service = service;
        _tepDinhKemService = tepDinhKemService;
    }
}
```

### Bước 2: Tải lên ảnh hiện trường sự cố khi người dùng gửi báo cáo
Trong action gửi báo cáo sự cố (hoặc trong `BaoCaoSuCoService`):
```csharp
// files là List<IFormFile> nhận từ form gửi sự cố
if (files != null && files.Any(f => f.Length > 0))
{
    var maTaiKhoan = HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
    var ketQua = await _tepDinhKemService.UploadNhieuTepAsync(
        files,
        LoaiDoiTuongDinhKemEnum.BaoCaoSuCo,
        maBaoCaoSuCo,
        maTaiKhoan,
        chiChapNhanAnh: true // Chỉ nhận ảnh .jpg, .png, .webp...
    );
}
```

### Bước 3: Lấy danh sách ảnh hiện trường để hiển thị trên trang Chi tiết sự cố / Phiếu xử lý
```csharp
var danhSachAnh = await _tepDinhKemService.LayDanhSachTheoDoiTuongAsync(
    LoaiDoiTuongDinhKemEnum.BaoCaoSuCo,
    maBaoCaoSuCo
);
```

### Bước 4: Xóa ảnh (nếu cần)
```csharp
await _tepDinhKemService.XoaTepAsync(maTepDinhKem);
```
*(Hàm sẽ tự động xóa bản ghi trong CSDL và xóa file vật lý tương ứng trên đĩa cứng)*.
