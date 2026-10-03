# Hệ thống Quản lý Bảo trì Thiết bị

Đề tài 16 — ASP.NET Core 10 MVC + Entity Framework Core + SQL Server.

Quản lý thiết bị, lập kế hoạch bảo trì định kỳ, tiếp nhận báo cáo sự cố, phân công kỹ thuật viên xử lý, nghiệm thu và thống kê chi phí.

# Xem chi tiết cấu trúc tại

- [Sơ đồ ERD](./docs/ERD.md)
- [Nội dung](./docs/THIETKE.md)

## Công nghệ sử dụng

- ASP.NET Core 10 MVC
- Entity Framework Core (Code First + Migration)
- SQL Server
- Bootstrap 5, jQuery, jQuery Validation

## Thành viên nhóm & phân công

| Thành viên        | Module   | Nội dung chính                                                    |
| ----------------- | -------- | ----------------------------------------------------------------- |
| SV1 (Trưởng nhóm) | Module 1 | Tài khoản, Đăng nhập/Phân quyền, Loại thiết bị, Khu vực           |
| SV2               | Module 2 | Thiết bị: CRUD, Tìm kiếm/Lọc/Sắp xếp/Phân trang, tính hạn bảo trì |
| SV3               | Module 3 | Kế hoạch bảo trì: CRUD, workflow trạng thái                       |
| SV4               | Module 4 | Báo cáo sự cố, Kỹ thuật viên, Phiếu xử lý, Phân công xử lý        |
| SV5               | Module 5 | Chi tiết xử lý, Nghiệm thu, Dashboard, Thống kê                   |

## Cấu trúc thư mục

```
QuanLyBaoTriThietBi_UNETI01_TI17A4HN (đang trong quá trình bổ sug)/
├── Data/                   # ApplicationDbContext, khai báo DbSet cho 10 bảng — tạo khi gộp Entity
├── Models/                 # Entity ánh xạ CSDL theo ERD, mỗi Entity 1 file, đúng 1 người phụ trách:
│   ├── TaiKhoan.cs / LoaiThietBi.cs / KhuVuc.cs
│   ├── ThietBi.cs
│   ├── KeHoachBaoTri.cs
│   ├── BaoCaoSuCo.cs / KyThuatVien.cs / PhieuXuLy.cs / PhanCongXuLy.cs
│   ├── ChiTietXuLy.cs
│   └── ...
├── ViewModels/...          # Lớp dữ liệu riêng cho View, không map CSDL — vd DashboardViewModel, ThongKeViewModel (SV5)
├── Controllers/...         # 1 Controller cho mỗi Entity chính, cùng người phụ trách như Models tương ứng
├── Views/                  # Giao diện .cshtml, chia theo Controller
│   ├── TaiKhoan/ LoaiThietBi/ KhuVuc/
│   ├── ThietBi/
│   ├── KeHoachBaoTri/
│   ├── BaoCaoSuCo/ KyThuatVien/ PhieuXuLy/
│   ├── Dashboard/
│   ├── Home/               # Trang chủ mặc định của template MVC
│   ├── Shared/             # Layout dùng chung — chỉ SV1 sửa để tránh conflict
│   └── ...
├── wwwroot/
│   ├── css/ js/            # Style/JS tuỳ chỉnh của project
│   └── lib/                # Thư viện ngoài (Bootstrap, jQuery...) — không tự sửa tay
├── Properties/             # launchSettings.json — cấu hình profile chạy debug
├── Program.cs              # Điểm khởi động ứng dụng, khai báo middleware/service
├── appsettings.json        # Cấu hình chung
├── appsettings.Development.json  # Connection String riêng máy dev — bị .gitignore, mỗi người tự tạo
└── *.csproj                # Khai báo project .NET, package NuGet, target framework
```

> Folder chứa file `.gitkeep` là folder tạo sẵn nhưng chưa có code — dành cho người phụ trách module đó thêm file vào, xoá `.gitkeep` sau khi đã có file thật. `Views/{Module}/*.cshtml` không cần tạo tay, Visual Studio tự sinh khi scaffold Controller (Add Controller with views using EF).

**Quy tắc quan trọng để tránh conflict Git:** mỗi Entity/Controller là 1 file riêng do đúng 1 người phụ trách theo bảng trên. Không sửa file của người khác — cần thay đổi thì nhắn người phụ trách hoặc merge qua Pull Request.

## Bắt đầu chạy project (cho thành viên mới clone/pull về)

```bash
git clone <link-repo>
cd QuanLyBaoTriThietBi_UNETI01_TI17A4HN
dotnet restore
```

Tạo file `appsettings.Development.json` (không có sẵn trong repo do bị `.gitignore`) với nội dung:

```json
// Chưa update
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=QuanLyBaoTriThietBi;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

Sau khi Module `Data/ApplicationDbContext` (đã có), chạy:

```bash
dotnet ef database update
```

rồi `dotnet run` hoặc F5 trong Visual Studio để khởi động.

## Quy ước code

- **Comment đầu mỗi file** (bắt buộc):
  ```csharp
  // Họ tên: Nguyễn Văn A
  // MSSV: 2000000
  // Nội dung: Entity ThietBi - Module 2
  ```
- **Tên class/file:** PascalCase, số ít, trùng tên Entity trong ERD (`ThietBi.cs`, không viết `ThietBis.cs`).
- **Tên biến/property:** PascalCase cho property C# (`TenThietBi`), camelCase cho biến cục bộ (`tenThietBi`).
- **Data Annotation:** dùng `[Required]`, `[StringLength]`, `[Range]`... ngay trong Entity, không kiểm tra validate rải rác nhiều chỗ.

## Quy ước nhánh Git

Mỗi SV code trên 1 nhánh riêng, **không code thẳng lên `main`**:

```
main                    ← nhánh chính, chỉ merge code đã chạy được
feature/sv1-module1
feature/sv2-module2
feature/sv3-module3
feature/sv4-module4
feature/sv5-module5
```

Quy trình mỗi lần code xong 1 phần việc:

```bash
git checkout main
git pull                          # cập nhật code mới nhất trước khi bắt đầu
git checkout -b feature/sv2-module2   # (chỉ tạo lần đầu, các lần sau: git checkout feature/sv2-module2)
# ... code ...
git add .
git commit -m "[SV2][Module2] Code CRUD ThietBi"
git push origin feature/sv2-module2
```

Sau đó vào GitHub tạo **Pull Request** vào `main`, nhờ SV1 (hoặc 1 bạn khác) review rồi mới bấm Merge. Không merge thẳng khi chưa ai xem qua.

## Quy ước commit message

Format: `[MãSV][Module] Nội dung ngắn gọn`

Ví dụ:

- `[SV1][Module1] Code CRUD TaiKhoan`
- `[SV3][Module3] Workflow trạng thái KeHoachBaoTri`
- `[Chung] Gộp Entity 5 thành viên, tạo Migration đầu tiên`

## Checklist trước khi merge vào main

- [ ] Code build được, không lỗi đỏ
- [ ] Đã pull `main` mới nhất và merge/giải quyết conflict trước khi tạo PR
- [ ] Đã ghi comment đầu file (Họ tên - MSSV - Nội dung)
- [ ] Đã test thử chức năng chạy đúng (không chỉ build được)
