# QUY ƯỚC GIAO DIỆN (MaintainPro)

## 1. Ba quy tắc cốt lõi

1. **Không hard-code màu, cỡ chữ, bo góc trong `.cshtml`.** Chỉ dùng class có sẵn trong `wwwroot/css/site.css` (hoặc class Bootstrap 5.3). Không `style="color:#..."`.
2. **Trạng thái → badge luôn qua `UiHelper` + `_StatusBadge`.** Không tự viết `if (TrangThai == ...) class="..."` trong View. Một trạng thái chỉ có **một** màu trên toàn hệ thống.
3. **Muốn thêm/đổi màu, class, component dùng chung → sửa `site.css` trong Pull Request riêng và báo nhóm.** Không ai sửa riêng trong `<style>` của từng View.

## 2. Các file liên quan

| File                               | Vai trò                                                                            |
| ---------------------------------- | ---------------------------------------------------------------------------------- |
| `wwwroot/css/site.css`             | Design token (CSS variables) + ghi đè Bootstrap 5.3.3 + component dùng chung       |
| `Helpers/UiHelper.cs`              | Ánh xạ enum → nhãn tiếng Việt + màu badge; định dạng tiền/ngày; tính "hạn bảo trì" |
| `Views/Shared/_StatusBadge.cshtml` | Partial vẽ badge từ `BadgeInfo`                                                    |
| `Views/Shared/_Layout.cshtml`      | Khung trang sau đăng nhập: sidebar + header + vùng nội dung + thông báo `TempData` |
| `Views/Shared/_Sidebar.cshtml`     | Menu theo vai trò (Admin / Kỹ thuật viên / Người sử dụng)                          |
| `Views/Shared/_AuthLayout.cshtml`  | Khung không có sidebar (Đăng nhập, 403/404/500)                                    |
| `docs/STITCH_PROMPT.md`            | Prompt gốc thiết kế 26 màn hình (tham khảo bố cục từng màn)                        |

## 3. Bảng màu (tóm tắt)

Dùng qua class hoặc biến `var(--tên)`; mã hex chỉ để tra cứu.

| Nhóm                 | Token                    | Hex                   | Dùng cho                                                             |
| -------------------- | ------------------------ | --------------------- | -------------------------------------------------------------------- |
| **Primary (cobalt)** | `--primary-500`          | `#1F5FBF`             | Nút chính, link, tab active, focus                                   |
|                      | `--primary-600` / `-700` | `#174E9E` / `#103F82` | Hover / pressed                                                      |
|                      | `--primary-800`          | `#0B2F63`             | Nền sidebar                                                          |
|                      | `--primary-50` / `-100`  | `#EEF4FC` / `#D9E6F8` | Nền hover, hàng được chọn, chip                                      |
| **Accent (cam)**     | `--accent-500`           | `#F58220`             | Nút nghiệp vụ chính ("Báo sự cố", "Tạo phiếu xử lý"), chấm thông báo |
|                      | `--accent-700`           | `#B3540A`             | **Chữ** cam trên nền trắng                                           |
| **Neutral**          | `--n-25`                 | `#F8FAFC`             | Nền trang                                                            |
|                      | `--n-0`                  | `#FFFFFF`             | Card, bảng, input                                                    |
|                      | `--n-100`                | `#E2E8F0`             | Viền card, đường kẻ                                                  |
|                      | `--n-500`                | `#64748B`             | Chữ phụ, placeholder                                                 |
|                      | `--n-600`                | `#5B6B80`             | Tiêu đề cột bảng                                                     |
|                      | `--n-900`                | `#0F172A`             | Chữ chính                                                            |
| **Ngữ nghĩa**        | `success`                | `#E7F6EC` / `#166534` | Hoàn thành, đang hoạt động                                           |
|                      | `warning`                | `#FFF6DB` / `#92600A` | Chờ xử lý, sắp đến hạn                                               |
|                      | `danger`                 | `#FDE9E7` / `#B42318` | Quá hạn, khẩn cấp, lỗi, xóa                                          |
|                      | `info`                   | `#E3F0FF` / `#1D4ED8` | Đang xử lý                                                           |
|                      | `muted`                  | `#EEF1F5` / `#475569` | Đã hủy, ngừng                                                        |
|                      | `purple`                 | `#F1EBFD` / `#5B2BB8` | **Chờ nghiệm thu**                                                   |

Ba lưu ý dễ sai:

- **Chữ trên nền cam `#F58220` phải là màu tối `#0F172A`** (chữ trắng không đủ tương phản). Dùng `.btn-accent`, đã đúng sẵn.
- **Không dùng `--n-300` (`#94A3B8`) cho chữ.** Chỉ dùng cho icon mờ.
- **Không chỉ dùng màu để truyền nghĩa:** badge luôn có chữ.

## 4. Trạng thái → badge (bắt buộc thống nhất)

Gọi: `<partial name="_StatusBadge" model="UiHelper.Badge(item.TrangThai)" />`

| Enum                                            | Giá trị → màu                                                                                                                         |
| ----------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------- |
| `TinhTrangThietBiEnum`                          | DangHoatDong **xanh lá** · DangBaoTri **xanh dương** · TamNgungDoSuCo **vàng (viền đậm)** · NgungSuDung **xám** (cả hàng mờ)          |
| `TrangThaiKeHoachEnum`                          | ChoThucHien vàng · DaPhanCong xanh dương · DangThucHien xanh dương đậm · **ChoNghiemThu tím** · HoanThanh xanh lá · DaHuy xám         |
| `TrangThaiBaoCaoEnum`                           | MoiBao vàng + chấm đỏ · DaTiepNhan xanh dương · DangXuLy xanh dương đậm · **ChoNghiemThu tím** · HoanThanh xanh lá · DaHuy xám        |
| `TrangThaiPhieuXuLyEnum`                        | ChoPhanCong vàng · DaPhanCong xanh dương · DangXuLy xanh dương đậm · **ChoNghiemThu tím** · HoanThanh xanh lá · DaHuy xám             |
| `TrangThaiPhanCongEnum`                         | DangHieuLuc xanh lá · DaKetThuc xám                                                                                                   |
| `MucDoSuCoEnum` (thẻ vuông, thanh màu bên trái) | Thap xám · TrungBinh xanh dương · Cao **cam** · KhanCap **đỏ đặc, chữ trắng**                                                         |
| `MucDoUuTienEnum` (mũi tên + chữ)               | Thap ↓ xám · TrungBinh – xanh dương · Cao ↑ cam                                                                                       |
| `VaiTroEnum` (chip)                             | Admin tím · KyThuatVien cobalt · NguoiSuDung xám                                                                                      |
| `KetQuaDangNhapEnum`                            | ThanhCong xanh lá · SaiMatKhau vàng · TaiKhoanBiKhoa đỏ · KhongTonTaiTaiKhoan xám                                                     |
| Cột `TrangThai` kiểu bit                        | `UiHelper.BadgeHoatDong(x)`: true xanh lá "Hoạt động"; false xám "Ngừng hoạt động" (với tài khoản: `laTaiKhoan: true` → đỏ "Bị khóa") |
| **Hạn bảo trì** (tính động)                     | `UiHelper.HanBaoTri(ngayTiepTheo)` → Chưa đến hạn xanh lá · Sắp đến hạn (≤ 7 ngày) vàng · Quá hạn đỏ, kèm ghi chú "quá N ngày"        |

## 5. Chữ, khoảng cách, định dạng

- **Font:** Be Vietnam Pro (chữ), JetBrains Mono (mã, serial, tiền, giờ). Dùng class `.mono` / `.num` (`.num` còn căn phải).
- **Cỡ chữ:** thân 14px; `h1` 24 · `h2` 20 · `h3` 16; nhãn cột bảng 12px IN HOA; số KPI 32px.
- **Khoảng cách:** bội của 4px (4, 8, 12, 16, 20, 24, 32). Bo góc: input/nút 8px, card 12px, modal 16px, badge tròn.
- **Tiền:** `UiHelper.Tien(x)` → `1.250.000 ₫`. **Ngày:** `UiHelper.Ngay(x)` → `07/10/2026`; **ngày giờ:** `UiHelper.NgayGio(x)`. **Mã hiển thị:** `UiHelper.Ma("PX", id)` → `PX-0007` (KH = kế hoạch, SC = sự cố, PX = phiếu xử lý).
- **Số và tiền trong bảng:** căn phải, font mono. Tên trong bảng: in đậm, dòng phụ (serial…) nhỏ và xám bên dưới (`.cell-title` + `.cell-sub`).

## 6. Component → class

| Cần                                                 | Dùng                                                                                                                                                                                                                                         |
| --------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Nút chính / phụ                                     | `btn btn-primary` / `btn btn-outline-secondary`                                                                                                                                                                                              |
| Nút nghiệp vụ nổi bật                               | `btn btn-accent` (chỉ **một** nút accent mỗi màn)                                                                                                                                                                                            |
| Nút nhẹ, xóa/khóa                                   | `btn btn-ghost` · `btn btn-danger` · `btn btn-outline-danger`                                                                                                                                                                                |
| Nút chỉ icon                                        | `btn btn-ghost btn-icon` (**bắt buộc** có `title`)                                                                                                                                                                                           |
| Form                                                | `form-label` (thêm `required` nếu bắt buộc) · `form-control` · `form-select` · `form-text`                                                                                                                                                   |
| Trường hệ thống tự tính (Thành tiền, Ngày báo cáo…) | `form-control is-system` + `readonly` + tooltip "Hệ thống tự tính"                                                                                                                                                                           |
| Lỗi validation                                      | `<span asp-validation-for="..."></span>` và `<div asp-validation-summary="All"></div>` — **không tự gắn class**: ASP.NET tự thêm `field-validation-error` / `validation-summary-errors` khi có lỗi, `site.css` đã tạo kiểu cho hai class này |
| Card / KPI                                          | `card` + `card-body` · `card kpi-card` (+ `is-warning` / `is-danger`)                                                                                                                                                                        |
| Bảng                                                | `<div class="table-card"><table class="data-table">…</table><div class="table-footer">…</div></div>` · hàng mờ: `<tr class="is-muted">`                                                                                                      |
| Thanh lọc                                           | `filter-bar` (ô tìm: `search`) · chip lọc: `filter-chips` > `filter-chip`                                                                                                                                                                    |
| Badge                                               | partial `_StatusBadge` (đừng tự viết `<span class="status-badge …">` nếu đã có enum)                                                                                                                                                         |
| Tab                                                 | `nav nav-tabs` (Bootstrap)                                                                                                                                                                                                                   |
| Stepper vòng đời                                    | `<ol class="stepper"><li class="step is-done">…</li><li class="step is-current">…</li><li class="step">…</li></ol>`                                                                                                                          |
| Timeline lịch sử                                    | `ul.timeline` > `li.timeline-item` + `is-maintenance` / `is-incident` / `is-done` / `is-reassign`                                                                                                                                            |
| Avatar                                              | `avatar` (`avatar-lg`, `is-accent`, `is-success`)                                                                                                                                                                                            |
| Trạng thái rỗng                                     | `empty-state`                                                                                                                                                                                                                                |
| Khung trang                                         | `app-shell` > `sidebar` + (`app-header` + `app-content`); đầu trang: `page-header` + `page-actions`                                                                                                                                          |
| Đăng nhập / lỗi                                     | `auth-split` > `auth-hero` + `auth-form`                                                                                                                                                                                                     |

## 7. Quy tắc nội dung & UX

- **100% tiếng Việt có dấu** (nhãn, nút, placeholder, thông báo). Nút là **động từ** rõ nghĩa: "Lưu thiết bị", "Gửi báo cáo" — không dùng "OK/Submit".
- Form: **[Hủy] bên trái, [Lưu] bên phải**.
- **Không hiện nút mà người dùng không có quyền** (ẩn hẳn bằng `User.IsInRole(...)`, không chỉ disable). Menu sidebar thay đổi theo vai trò.
- **Không xóa vật lý dữ liệu có lịch sử:** nút chính là "Ngừng sử dụng / Hủy / Khóa". Xóa luôn qua modal xác nhận.
- Giữ nguyên bộ lọc/tìm kiếm/sắp xếp khi phân trang và khi quay lại từ trang chi tiết.
- Mỗi danh sách phải có đủ: **trạng thái rỗng**, **không có kết quả lọc**, và **thông báo thành công/lỗi** (toast hoặc alert).
- Hàng bị ngừng/hủy/khóa: `is-muted`, ẩn nút Sửa.

## 8. Khác so với prompt Stitch gốc (đã chỉnh có chủ đích)

| Prompt gốc                                                   | Quy ước dùng                                                                                                                                  | Lý do                                                   |
| ------------------------------------------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------- |
| Placeholder `#94A3B8`                                        | `#64748B` (`--n-500`)                                                                                                                         | Gốc chỉ đạt 2,56:1; chuẩn AA cần ≥ 4,5:1                |
| Tiêu đề cột bảng `#64748B` trên nền `#F1F5F9`                | `#5B6B80` (`--n-600`)                                                                                                                         | Gốc 4,34:1 (hụt AA); mới 4,97:1                         |
| Icon Lucide (prompt)                                         | **Material Symbols Outlined** (mockup Stitch đang dùng, đã nạp sẵn trong `_Layout`): `<span class="material-symbols-outlined">warning</span>` | Thống nhất một bộ icon; Stitch sinh ra Material Symbols |
| Tên class `.badge-success`, `.btn-primary`… (Stitch sinh ra) | `.status-badge.is-success`, Bootstrap `.btn-*`…                                                                                               | Tránh trùng/đụng tên với Bootstrap 5; bảng ở mục 6      |

**Phạm vi:** prompt gốc mô tả 26 màn hình, trong đó có vài thứ **không nằm trong thiết kế CSDL / yêu cầu bắt buộc**. Xem là **tùy chọn, làm sau cùng nếu còn thời gian**: Dark mode, Ctrl+K tìm toàn hệ thống, xem Lịch cho kế hoạch, Kanban "Việc của tôi", chọn nhiều dòng (checkbox), mã QR thiết bị, xuất Excel. Ảnh đính kèm dùng bảng `TepDinhKem` (đã có); mã QR có thể sinh từ mã thiết bị, không cần thêm cột.

## 9. Mẫu prompt khi nhờ AI viết View

Dán nguyên khối dưới đây, rồi thêm yêu cầu cụ thể:

```
Viết file Razor .cshtml cho project ASP.NET Core 10 MVC + Bootstrap 5.3.3 + jQuery.
Tuân thủ nghiêm docs/GIAO_DIEN.md (tôi dán kèm bên dưới) và wwwroot/css/site.css.

Bắt buộc:
- Không hard-code màu/cỡ chữ/bo góc, không style="" và không <style> trong View. Chỉ dùng class có trong site.css hoặc Bootstrap 5.3.
- Badge trạng thái: <partial name="_StatusBadge" model="UiHelper.Badge(...)" />. Hạn bảo trì: UiHelper.HanBaoTri(...).
- Tiền/ngày: UiHelper.Tien / UiHelper.Ngay / UiHelper.NgayGio. Mã hiển thị: UiHelper.Ma("PX", id).
- 100% tiếng Việt có dấu. Nút là động từ. Form: [Hủy] trái, [Lưu] phải.
- Bảng dùng <div class="table-card"><table class="data-table">; số/tiền class="num"; tên dùng .cell-title + .cell-sub.
- Form dùng asp-for, <span asp-validation-for="..."></span> và <div asp-validation-summary="All"></div> (không tự gắn class, ASP.NET tự thêm khi có lỗi).
- Trường hệ thống tự tính: class "form-control is-system" + readonly.
- Ẩn nút theo quyền bằng User.IsInRole(...).
- Icon: Material Symbols Outlined (<span class="material-symbols-outlined">ten_icon</span>). Thanh tiến độ: bar-row / bar-track / bar-fill. Thông báo sau khi lưu: TempData["ThanhCong"] / TempData["Loi"] rồi RedirectToAction (không tự viết alert).
- Namespace/model: QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models (enum trong .Models.Enums). Không đổi Entity/DbContext.

Yêu cầu cụ thể: <mô tả màn hình, model/ViewModel, các cột, bộ lọc...>
```

## 10. Khi giao diện cần thay đổi

1. Báo nhóm trước khi đổi token màu / thêm component dùng chung.
2. Một người sửa `site.css` (và `UiHelper.cs` nếu liên quan enum) trong một Pull Request; ghi rõ thay đổi.
3. Cập nhật mục tương ứng trong file này cùng PR.

## 11. Khung trang và menu theo vai trò

- Mọi trang sau đăng nhập dùng `_Layout` (mặc định qua `_ViewStart`). Đăng nhập và trang lỗi: `@{ Layout = "_AuthLayout"; }`.
- Tiêu đề trang: `@{ ViewData["Title"] = "Danh sách thiết bị"; }` → hiện ở tab trình duyệt và breadcrumb.
- Menu ở `_Sidebar.cshtml` đã liệt kê sẵn theo 3 vai trò. Tên controller/action trong đó là **dự kiến**; mỗi người đổi cho khớp controller của module mình (ví dụ `PhieuXuLy/ViecCuaToi`, `BaoCaoSuCo/CuaToi`). Thêm mục mới: một dòng `Muc("Controller", "Action", "ten_icon", "Nhãn");`.
- Thông báo sau khi lưu: controller gán `TempData["ThanhCong"] = "Đã lưu thiết bị thành công."` hoặc `TempData["Loi"] = "Không thể xóa: loại thiết bị đang có thiết bị sử dụng."` rồi `RedirectToAction`. `_Layout` tự hiển thị alert xanh/đỏ, không cần viết lại ở từng View.
- Đầu mỗi trang nội dung:
  ```html
  <div class="page-header">
    <div>
      <h1>Danh sách thiết bị</h1>
      <p>Mô tả ngắn…</p>
    </div>
    <div class="page-actions">
      <a class="btn btn-primary" asp-action="Create">Thêm thiết bị</a>
    </div>
  </div>
  ```

## 12. Icon, biểu đồ, thanh tiến độ

- **Icon:** Material Symbols Outlined, 20px mặc định (`icon-16`, `icon-24` để đổi cỡ). Nút chỉ icon bắt buộc có `title`.
- **Thanh tiến độ ngang** (biểu đồ cột ngang trên Dashboard) dùng CSS thuần: `.bar-row` > `.bar-head` (nhãn + số) + `.bar-track` > `.bar-fill`. Chiều rộng là giá trị động nên **được phép** dùng `style="width:@pct%"` (đây là ngoại lệ duy nhất cho quy tắc "không style inline").
- **Màu cột theo trạng thái** (phải khớp với badge, mục 4):

  | Nhóm trạng thái                         | Class `.bar-fill` |
  | --------------------------------------- | ----------------- |
  | Chờ… (ChoThucHien, ChoPhanCong, MoiBao) | `is-warning`      |
  | Đã phân công / Đã tiếp nhận             | `is-info`         |
  | Đang xử lý / Đang thực hiện             | `is-info-strong`  |
  | Chờ nghiệm thu                          | `is-purple`       |
  | Hoàn thành                              | `is-success`      |
  | Đã hủy                                  | `is-muted`        |

- **Donut / đường:** đề xuất dùng **Chart.js 4** (CDN jsdelivr) vì project chưa có thư viện biểu đồ; nhóm xác nhận trước khi dùng. Màu series theo thứ tự: `#1F5FBF`, `#F58220`, `#16A34A`, `#7C3AED`, `#0EA5A4`, `#DC2626`, `#94A3B8` (tối đa 6 màu/biểu đồ). Donut tình trạng thiết bị dùng màu badge: xanh lá / cobalt / vàng / xám. Trục tiền ghi `12,2 tr`, không ghi `12.2M`.

## 13. Port từ mockup HTML (Tailwind) sang Razor

Mockup do Stitch sinh ra dùng **Tailwind CDN**; project dùng **Bootstrap + site.css**. Màu, font và kích thước khung (sidebar 264px, header 64px) trong mockup **khớp token** của quy ước, nên có thể lấy bố cục làm chuẩn. Nhưng **không copy nguyên HTML**. Quy đổi:

| Mockup (Tailwind)                                                | Project                                                               |
| ---------------------------------------------------------------- | --------------------------------------------------------------------- |
| `bg-neutral-0 rounded-xl p-5 shadow-sm`                          | `card` + `card-body` (KPI: `card kpi-card`)                           |
| Khối `<span class="inline-flex … rounded-full bg-success-bg …">` | partial `_StatusBadge` + `UiHelper.Badge(...)`                        |
| `w-8 h-8 rounded-full bg-primary-100 …` + chữ viết tắt           | `<span class="avatar">@UiHelper.VietTat(ten)</span>`                  |
| `<table class="w-full …">` với `thead` tự tô                     | `table-card` > `data-table`                                           |
| `flex items-center gap-2` / `grid grid-cols-3 gap-6`             | Bootstrap: `d-flex align-items-center gap-2` / `row g-4` + `col-lg-4` |
| `font-mono-code`                                                 | `mono` hoặc `num`                                                     |
| Thanh `<div class="bg-… h-2 rounded-full" style="width:…">`      | `bar-row` / `bar-track` / `bar-fill is-…`                             |

**Các chỗ mockup lệch quy ước hoặc không có dữ liệu thật (đừng làm theo):**

| #   | Mockup                                                                                                                     | Xử lý                                                                                                                                                                                                                                                                          |
| --- | -------------------------------------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| 1   | Logo lặp ở cả sidebar và header; breadcrumb "MaintainPro / Hệ thống quản trị"                                              | Logo chỉ ở sidebar; breadcrumb theo trang (đã làm trong `_Layout`)                                                                                                                                                                                                             |
| 2   | Card chỉ có shadow, không viền; nút phụ không viền                                                                         | Dùng `.card` (có viền `#E2E8F0`) và `btn-outline-secondary`                                                                                                                                                                                                                    |
| 3   | Chữ 10–11px (nhãn "Khả dụng", tiêu đề nhóm menu, chip)                                                                     | Tối thiểu 12px                                                                                                                                                                                                                                                                 |
| 4   | Cột "Đã tiếp nhận" (báo cáo) và "Đã phân công" (phiếu) màu **cam**                                                         | Phải là xanh dương (bảng mục 12); cam chỉ dành cho mức độ Cao và hành động nghiệp vụ                                                                                                                                                                                           |
| 5   | Số liệu mâu thuẫn: donut 21/30 = 70% nhưng chân thẻ ghi "83,3%"; phiếu xử lý ghi "24 phiếu" trong khi các dòng cộng lại 26 | Số liệu thật lấy từ truy vấn, tính phần trăm trên đúng tổng                                                                                                                                                                                                                    |
| 6   | "vi phạm SLA", "Hạn mức dự trù 25.000.000 ₫", "Tiết kiệm 26,2% so với ngân sách trần", "Trực tuyến", "đồng bộ mỗi 5 phút"  | **Bỏ**: CSDL không có SLA hay ngân sách                                                                                                                                                                                                                                        |
| 7   | "Hoạt động gần đây" với vai trò "Điều phối viên", "Giám sát ca trực", mã `TB-DK-019`, `SC-2026-0043`, thiết bị Cummins…    | Hệ thống chỉ có 3 vai trò; **CSDL không có bảng nhật ký nghiệp vụ** (`NhatKyDangNhap` chỉ ghi đăng nhập). Nếu làm, tổng hợp từ `BaoCaoSuCo.NgayBaoCao`, `PhieuXuLy.NgayTao/NgayBatDau/NgayHoanThanh`, `PhanCongXuLy.NgayPhanCong`, `ChiTietXuLy.NgayCapNhat`; hoặc bỏ khối này |
| 8   | Biểu đồ đường SVG vẽ tay với số cố định, nền gradient                                                                      | Dùng Chart.js (mục 12), dữ liệu từ truy vấn                                                                                                                                                                                                                                    |
| 9   | Nhãn nhỏ in hoa trên mỗi KPI ("Quy mô hạ tầng", "Cảnh báo sớm"…)                                                           | Bỏ cho gọn; chỉ giữ nhãn + số + một dòng ghi chú tính được từ dữ liệu                                                                                                                                                                                                          |
| 10  | Mục menu thường chữ đậm 600; mục active thiếu thanh cam                                                                    | `.sidebar-link` đã xử lý đúng (500 / 600 khi active, thanh cam 3px)                                                                                                                                                                                                            |

Dữ liệu mẫu trong mockup (tên người, thiết bị) khác với `DbSeeder`. Khi viết View thật, **dùng dữ liệu từ DB**; để kiểm tra giao diện, dùng chính dữ liệu seed.
