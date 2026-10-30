# DeviceMaster - Hệ Thống Quản Lý & Bảo Trì Thiết Bị (Equipment Maintenance System)

> **Hệ thống web toàn diện hỗ trợ số hóa quy trình quản lý trang thiết bị, lập kế hoạch bảo trì định kỳ, ghi nhận & điều phối xử lý sự cố kỹ thuật, nghiệm thu và thống kê báo cáo chi phí vận hành cho doanh nghiệp / tổ chức.**

---

## Mục Lục (Table of Contents)

- [Giới Thiệu Dự Án](#-giới-thiệu-dự-án)
- [Công Nghệ Sử Dụng (Tech Stack)](#-công-nghệ-sử-dụng-tech-stack)
- [Tính Năng Chính (Key Features)](#-tính-năng-chính-key-features)
- [Cấu Trúc Thư Mục (Project Structure)](#-cấu-trúc-thư-mục-project-structure)
- [Tài Khoản Trải Nghiệm (Test Accounts)](#-tài-khoản-trải-nghiệm-test-accounts)
- [Hướng Dẫn Cài Đặt (Getting Started)](#-hướng-dẫn-cài-đặt-getting-started)
- [Tài Liệu Kỹ Thuật (Architecture & Docs)](#-tài-liệu-kỹ-thuật-architecture--docs)
- [Giấy Phép (License)](#-giấy-phép-license)

---

## Giới Thiệu Dự Án

**DeviceMaster** là giải pháp phần mềm quản trị bảo trì thiết bị chuẩn hóa được xây dựng trên nền tảng **ASP.NET Core MVC (.NET 10)**. Hệ thống giải quyết trọn vẹn bài toán vòng đời thiết bị: từ tiếp nhận, định danh phân loại, bố trí theo khu vực/phòng ban, lên lịch bảo dưỡng tự động, tới luồng phân công nhiệm vụ kỹ thuật viên khi phát sinh hỏng hóc và tổng hợp KPI/chi phí minh bạch cho bộ phận quản lý.

- **Phân loại dự án:** Đồ án tốt nghiệp / Dự án ứng dụng doanh nghiệp thực tế.
- **Nền tảng mục tiêu:** .NET 10.0 (Web SDK).
- **Mô hình kiến trúc:** MVC Pattern tích hợp Repository & Service Layer, Custom Cookie/Session Authentication Handler.

---

## Công Nghệ Sử Dụng (Tech Stack)

| Thành phần (Component)    | Công nghệ (Technology)               | Mô tả & Mục đích sử dụng (Description)                                                     |
| :------------------------ | :----------------------------------- | :----------------------------------------------------------------------------------------- |
| **Runtime & Core**        | .NET 10 (C# 13)                      | Nền tảng phát triển web hiệu năng cao, bảo mật và hiện đại nhất từ Microsoft               |
| **Web Architecture**      | ASP.NET Core MVC                     | Kiến trúc tách biệt rõ ràng Model - View - Controller, hỗ trợ Dependency Injection mạnh mẽ |
| **Data Access & ORM**     | Entity Framework Core 10             | Tiếp cận Code-First, tự động đồng bộ Migration và Seeding dữ liệu kiểm thử                 |
| **Database Engine**       | Microsoft SQL Server                 | Cơ sở dữ liệu quan hệ lưu trữ an toàn, truy vấn tối ưu và toàn vẹn dữ liệu (ACID)          |
| **Authentication & Auth** | Cookie/Session Auth + Custom Handler | Cơ chế xác thực tùy biến thông qua Session Middleware, băm mật khẩu `IPasswordHasher`      |
| **Design Pattern**        | Repository & Service Pattern         | Tách biệt nghiệp vụ (Business Logic) và tầng truy cập dữ liệu, dễ dàng mở rộng & Unit Test |
| **Frontend UI/UX**        | Razor View, Bootstrap 5              | Giao diện Responsive đa thiết bị, thành phần UI hiện đại, thân thiện người dùng            |
| **Client Scripts**        | jQuery, AJAX, Validation             | Thao tác DOM linh hoạt, gọi API ngầm không reload trang, validate dữ liệu 2 chiều          |
| **Data Presentation**     | DataTables.net, Chart.js             | Bảng dữ liệu hỗ trợ phân trang, lọc, sắp xếp tức thì; trực quan hóa đồ thị thống kê        |

---

## Tính Năng Chính (Key Features)

### Phía Người Sử Dụng / Nhân Viên (Staff & Client)

- **Quản lý danh sách thiết bị phụ trách:**
  - Tra cứu, tìm kiếm nhanh thiết bị theo mã định danh, loại thiết bị hoặc vị trí lắp đặt.
  - Xem thông tin chi tiết: cấu hình, thông số kỹ thuật, lịch sử bảo dưỡng và tình trạng hoạt động.
- **Gửi yêu cầu & Báo cáo sự cố (Incident Reporting):**
  - Tạo phiếu báo cáo sự cố hư hỏng kèm mô tả hiện trường, mức độ ưu tiên và hình ảnh minh chứng.
  - Theo dõi tiến độ xử lý sự cố theo thời gian thực (Chờ tiếp nhận -> Đang sửa -> Đã khắc phục).
- **Quản lý tài khoản cá nhân:**
  - Đăng nhập, cập nhật thông tin cá nhân và thay đổi mật khẩu bảo mật.

### Phía Kỹ Thuật Viên (Technician Workspace)

- **Tiếp nhận & Điều phối phiếu xử lý:**
  - Tiếp nhận công việc được phân công từ hệ thống hoặc trưởng nhóm kỹ thuật.
  - Cập nhật trạng thái phiếu: Đang xử lý, Chờ vật tư thay thế, Hoàn thành.
- **Ghi nhận quy trình sửa chữa & Vật tư:**
  - Khai báo nhật ký xử lý, thời gian khắc phục và ghi chú nguyên nhân sự cố.
  - Ghi nhận danh mục vật tư/linh kiện thay thế cùng chi phí thực tế phát sinh.
- **Nghiệm thu kỹ thuật:**
  - Lập biên bản nghiệm thu trước khi bàn giao thiết bị lại cho người dùng.

### Phía Quản Trị Viên (Admin & Manager Dashboard)

- **Bảng điều khiển thông minh (Analytics Dashboard):**
  - Biểu đồ trực quan tỉ lệ thiết bị: Đang hoạt động, Đang hỏng, Chờ bảo trì.
  - Thống kê chi phí bảo trì phát sinh theo tháng/quý/năm.
  - Đo lường thời gian trung bình xử lý sự cố (MTTR) của đội ngũ kỹ thuật.
- **Quản trị danh mục & Vòng đời thiết bị (Asset Lifecycle):**
  - CRUD danh mục Thiết bị, Loại thiết bị, Khu vực / Tòa nhà / Phòng ban.
  - Tự động cảnh báo và tính toán chu kỳ bảo dưỡng định kỳ tiếp theo.
- **Lập kế hoạch bảo trì phòng ngừa (Preventive Maintenance):**
  - Lên lịch bảo dưỡng hàng loạt theo chu kỳ (3 tháng, 6 tháng, 1 năm).
  - Phân công kỹ thuật viên phụ trách theo từng đợt bảo dưỡng.
- **Quản lý phân quyền & Người dùng (RBAC):**
  - Quản lý danh sách tài khoản, khóa/mở khóa trạng thái hoạt động.
  - Phân quyền 3 cấp độ: `Admin` (Toàn quyền), `KyThuatVien` (Kỹ thuật), `NguoiSuDung` (Nhân viên).

---

## Cấu Trúc Thư Mục (Project Structure)

```text
QuanLyBaoTriThietBi/
├── docs/                                   # Tài liệu dự án (ERD, sơ đồ kiến trúc, tài liệu thiết kế)
│   ├── ERD.md                              # Mô hình quan hệ thực thể cơ sở dữ liệu
│   └── THIETKE.md                          # Tài liệu đặc tả kỹ thuật & phân tích thiết kế
├── QuanLyBaoTriThietBi_UNETI01_TI17A4HN/  # Thư mục mã nguồn chính (Source Code)
│   ├── Controllers/                        # Xử lý Request/Response theo từng module
│   │   ├── Auth/                           # Controller đăng nhập, đăng xuất, đổi mật khẩu
│   │   ├── ThietBi/                        # Quản lý danh mục & trạng thái thiết bị
│   │   ├── BaoCaoSuCo/                     # Tiếp nhận & luân chuyển báo cáo sự cố
│   │   ├── KeHoachBaoTri/                  # Quản lý kế hoạch bảo dưỡng định kỳ
│   │   ├── PhieuXuLy/                      # Điều phối & phân công kỹ thuật viên
│   │   ├── ChiTietXuLy/                    # Ghi nhận vật tư & chi phí sửa chữa
│   │   ├── Dashboard/                      # Tổng hợp dữ liệu & chỉ số KPI vận hành
│   │   └── ThongKe/                        # Xuất báo cáo thống kê chuyên sâu
│   ├── Data/                               # Tầng cơ sở dữ liệu
│   │   ├── AppDbContext.cs                 # Entity Framework DbContext & cấu hình quan hệ (Fluent API)
│   │   └── DbSeeder.cs                     # Tự động khởi tạo dữ liệu mẫu khi DB trống
│   ├── Migrations/                         # Lịch sử EF Core Migrations
│   ├── Models/                             # Các Domain Entities (POCO) map với Database
│   │   ├── Enums/                          # Enum định nghĩa trạng thái, vai trò hệ thống
│   │   ├── TaiKhoan.cs                     # Thực thể tài khoản & phân quyền
│   │   ├── ThietBi.cs                      # Thực thể thiết bị & thông số kỹ thuật
│   │   └── ...                             # Các entity khác: KeHoachBaoTri, PhieuXuLy...
│   ├── Repositories/                       # Repository Pattern: Trừu tượng hóa truy xuất dữ liệu
│   ├── Services/                           # Business Logic Layer: Xử lý logic nghiệp vụ
│   ├── ViewModels/                         # Data Transfer Objects dành riêng cho Razor Views
│   ├── Helpers/                            # Tiện ích mở rộng, Session Handler, Authentication Filter
│   ├── Views/                              # Giao diện người dùng Razor Pages (.cshtml)
│   │   ├── Shared/                         # Layout chung (_Layout.cshtml), Sidebar, Navbar
│   │   └── [Module]/                       # Thư mục view CRUD tương ứng với từng Controller
│   ├── wwwroot/                            # Static Files: CSS, JS tùy biến, Vendor libraries, Images
│   ├── appsettings.json                    # Cấu hình hệ thống chung (Logging, Settings)
│   ├── appsettings.Development.json        # Cấu hình môi trường dev (Connection String)
│   ├── Program.cs                          # Khởi tạo DI Container, Middleware Pipeline & Run App
│   └── *.csproj                            # Cấu hình thư viện NuGet & Target Framework .NET 10
└── README.md                               # Tài liệu tổng quan dự án
```

---

## Tài Khoản Trải Nghiệm (Test Accounts)

Hệ thống đã tích hợp sẵn cơ chế **Auto-Seeding** dữ liệu mẫu ngay trong lần khởi chạy đầu tiên. Mật khẩu mặc định chung cho toàn bộ tài khoản thử nghiệm là: `123456`.

| Vai trò (Role)                 | Tên đăng nhập (Username) | Email               | Mật khẩu (Password) | Quyền hạn chính                                                         |
| :----------------------------- | :----------------------- | :------------------ | :------------------ | :---------------------------------------------------------------------- |
| **Quản trị viên (Admin)**      | `admin`                  | `admin@congty.vn`   | `123456`            | Toàn quyền cấu hình, quản lý người dùng, duyệt kế hoạch & xem Dashboard |
| **Kỹ thuật viên (Technician)** | `ktv_hung`               | `hung.nv@congty.vn` | `123456`            | Nhận phiếu sự cố, cập nhật vật tư thay thế, báo cáo hoàn tất sửa chữa   |
| **Kỹ thuật viên 2**            | `ktv_tuan`               | `tuan.tm@congty.vn` | `123456`            | Tương tự kỹ thuật viên chính, phối hợp xử lý thiết bị mạng / phần cứng  |
| **Người sử dụng (Staff)**      | `nsd_lan`                | `lan.vt@congty.vn`  | `123456`            | Xem thiết bị đơn vị mình phụ trách, gửi báo cáo sự cố & theo dõi xử lý  |

---

## Hướng Dẫn Cài Đặt (Getting Started)

Thực hiện lần lượt các bước dưới đây để thiết lập môi trường và chạy dự án trên máy cá nhân:

### 1. Yêu cầu tiên quyết (Prerequisites)

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) hoặc mới hơn.
- [Microsoft SQL Server](https://www.microsoft.com/sql-server/) (SQL Express, LocalDB hoặc Enterprise).
- [Visual Studio 2022 / 2025](https://visualstudio.microsoft.com/) (với workload _ASP.NET and web development_) hoặc [Visual Studio Code](https://code.visualstudio.com/) với C# Dev Kit.

---

### 2. Tải mã nguồn về máy (Clone Repository)

Mở cửa sổ dòng lệnh (Terminal / PowerShell) và chạy lệnh:

```bash
git clone https://github.com/your-username/QuanLyBaoTriThietBi.git
cd QuanLyBaoTriThietBi/QuanLyBaoTriThietBi_UNETI01_TI17A4HN
```

---

### 3. Phục hồi các gói phụ thuộc (Restore Dependencies)

```bash
dotnet restore
```

---

### 4. Cấu hình Chuỗi Kết Nối Cơ Sở Dữ Liệu (Connection String)

Tạo hoặc mở file `appsettings.Development.json` tại thư mục chứa dự án và điều chỉnh chuỗi kết nối phù hợp với SQL Server cục bộ của bạn:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=QuanLyBaoTriThietBi;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

_(Nếu sử dụng SQL Server Authentication, hãy thay đổi thành dạng: `Server=localhost;Database=QuanLyBaoTriThietBi;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True`)_.

---

### 5. Cập nhật Cơ sở dữ liệu (Database Migration & Seeding)

> **Ghi chú tự động:** Khi ứng dụng khởi động, phương thức `db.Database.Migrate()` và `DbSeeder.Seed(db)` trong `Program.cs` sẽ **tự động kiểm tra, khởi tạo database và nạp dữ liệu mẫu** nếu database chưa có dữ liệu.

Nếu bạn muốn chủ động tạo database trước từ CLI, hãy chạy:

```powershell
dotnet ef database update
```

---

### 6. Khởi chạy ứng dụng (Run Application)

Chạy lệnh sau trên terminal:

```bash
dotnet run
```

Hoặc nhấn **F5** / **Ctrl + F5** nếu thao tác trực tiếp trên Visual Studio.

---

### 7. Truy cập hệ thống (Access Application)

Mở trình duyệt web bất kỳ và điều hướng tới địa chỉ mặc định:

```text
http://localhost:5053
```

Đăng nhập bằng một trong các tài khoản trong bảng [Tài Khoản Trải Nghiệm](#-tài-khoản-trải-nghiệm-test-accounts) (ví dụ: `admin` / `123456`) để bắt đầu khám phá hệ thống.

---

## Tài Liệu Kỹ Thuật (Architecture & Docs)

- **Mô hình thực thể liên kết (ERD):** [Xem chi tiết tại docs/ERD.md](docs/ERD.md)
- **Đặc tả quy trình và kiến trúc hệ thống:** [Xem chi tiết tại docs/THIETKE.md](docs/THIETKE.md)

---

## Giấy Phép (License)

Dự án được phân phối dưới giấy phép mã nguồn mở [MIT License](LICENSE). Mọi đóng góp, thảo luận và báo cáo lỗi (Issues/Pull Requests) đều được hoan nghênh!

---

<p align="center">
  Phát triển với bởi <b>Nhóm sinh viên CNTT UNETI</b> • © 2026 DeviceMaster
</p>
