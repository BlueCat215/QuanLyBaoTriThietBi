// Họ tên: Nguyễn Văn Hoàng
// MSSV: 23103100303
// Nội dung: Module 1 - Đọc Session mỗi request và dựng ClaimsPrincipal (để User/IsInRole/[Authorize] và _Layout, _Sidebar hoạt động)
#nullable enable
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

/// <summary>
/// Cơ chế đăng nhập dùng Session: AuthController ghi MaTaiKhoan/HoTen/VaiTro vào Session,
/// handler này đọc lại và tạo claims cho mọi request. Không dùng cookie authentication.
/// </summary>
public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Session";

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder) { }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var session = Context.Features.Get<ISessionFeature>()?.Session;
        if (session is null)
            return AuthenticateResult.NoResult();

        await session.LoadAsync(Context.RequestAborted);

        var maTaiKhoan = session.GetInt32(SessionKeys.MaTaiKhoan);
        var vaiTro = session.GetString(SessionKeys.VaiTro);
        if (maTaiKhoan is null || string.IsNullOrEmpty(vaiTro))
            return AuthenticateResult.NoResult();

        var hoTen = session.GetString(SessionKeys.HoTen) ?? string.Empty;
        var tenDangNhap = session.GetString(SessionKeys.TenDangNhap) ?? hoTen;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, maTaiKhoan.Value.ToString()),
            new("MaTaiKhoan", maTaiKhoan.Value.ToString()),   // TaiKhoanController đọc claim này
            new(ClaimTypes.Name, tenDangNhap),
            new("HoTen", hoTen),                                // _Layout/_Sidebar đọc claim này
            new(ClaimTypes.Role, vaiTro)                        // = tên VaiTroEnum (Admin/KyThuatVien/NguoiSuDung)
        };

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName));
    }

    // Chưa đăng nhập mà vào trang có [Authorize] -> chuyển tới trang đăng nhập
    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var url = "/Auth/DangNhap";
        if (HttpMethods.IsGet(Request.Method))
            url += "?returnUrl=" + Uri.EscapeDataString(Request.Path + Request.QueryString);

        Response.Redirect(url);
        return Task.CompletedTask;
    }

    // Đã đăng nhập nhưng không đủ quyền -> trang 403
    protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.Redirect("/Auth/TuChoiTruyCap");
        return Task.CompletedTask;
    }
}
