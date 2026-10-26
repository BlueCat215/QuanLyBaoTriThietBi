// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Custom Authorization Filter kiểm tra đăng nhập và phân quyền người dùng
#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

/// <summary>
/// Custom Authorization Filter thay thế hoàn toàn cho [Authorize] mặc định của ASP.NET Core.
/// 
/// Cú pháp sử dụng:
/// 1. Yêu cầu đăng nhập: [CustomAuthorizationFilter]
/// 2. Phân quyền theo Roles dạng chuỗi: [CustomAuthorizationFilter(Roles = "Admin")]
/// 3. Phân quyền theo Roles nhiều vai trò: [CustomAuthorizationFilter(Roles = "Admin,KyThuatVien")]
/// 4. Phân quyền theo Enum: [CustomAuthorizationFilter(VaiTroEnum.Admin)]
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CustomAuthorizationFilterAttribute : Attribute, IAuthorizationFilter
{
    private readonly List<string> _vaiTroChoPhep = new();

    /// <summary>
    /// Thuộc tính Roles dạng chuỗi (ví dụ: Roles = "Admin" hoặc Roles = "Admin,KyThuatVien")
    /// </summary>
    public string Roles
    {
        get => string.Join(",", _vaiTroChoPhep);
        set
        {
            _vaiTroChoPhep.Clear();
            if (!string.IsNullOrWhiteSpace(value))
            {
                var roles = value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                _vaiTroChoPhep.AddRange(roles);
            }
        }
    }

    /// <summary>
    /// Constructor không tham số: Bắt buộc đăng nhập bất kể vai trò nào
    /// </summary>
    public CustomAuthorizationFilterAttribute()
    {
    }

    /// <summary>
    /// Constructor nhận Enum: Type-safe, tránh gõ sai chính tả
    /// </summary>
    public CustomAuthorizationFilterAttribute(params VaiTroEnum[] vaiTroChoPhep)
    {
        if (vaiTroChoPhep != null && vaiTroChoPhep.Length > 0)
        {
            _vaiTroChoPhep.AddRange(vaiTroChoPhep.Select(v => v.ToString()));
        }
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // 1. Bỏ qua kiểm tra nếu Action hoặc Controller có gắn [AllowAnonymous]
        var hasAllowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()
            || context.Filters.OfType<IAllowAnonymousFilter>().Any();

        if (hasAllowAnonymous)
        {
            return;
        }

        // 2. Kiểm tra phiên đăng nhập từ Session
        var session = context.HttpContext.Session;
        var maTaiKhoan = session.GetInt32(SessionKeys.MaTaiKhoan);
        var vaiTroStr = session.GetString(SessionKeys.VaiTro);

        if (maTaiKhoan == null || string.IsNullOrEmpty(vaiTroStr))
        {
            // Chưa đăng nhập -> Chuyển hướng tới trang Đăng nhập kèm ReturnUrl
            var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
            context.Result = new RedirectToActionResult("DangNhap", "Auth", new { returnUrl });
            return;
        }

        // 3. Nếu có quy định vai trò cụ thể -> Kiểm tra vai trò của người dùng
        if (_vaiTroChoPhep.Count > 0)
        {
            var hopLe = _vaiTroChoPhep.Any(r => string.Equals(r, vaiTroStr, StringComparison.OrdinalIgnoreCase));
            if (!hopLe)
            {
                // Đã đăng nhập nhưng không đủ quyền -> Chuyển sang trang 403 Từ chối truy cập
                context.Result = new RedirectToActionResult("TuChoiTruyCap", "Auth", null);
                return;
            }
        }
    }
}
