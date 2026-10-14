#nullable enable
namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

public static class ServiceCollectionExtensions
{
    /// Quét assembly, với mỗi class thuộc namespace *.Repositories hoặc *.Services có tên kết thúc bằng
    /// "Repository"/"Service" và có interface tên "I" + tên class (vd TaiKhoanService -> ITaiKhoanService)
    /// thì đăng ký AddScoped. Thêm file mới đúng quy ước là tự đăng ký, không cần sửa Program.cs.
    public static IServiceCollection AddRepositoriesAndServices(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        var implementations = assembly
            .GetTypes()
            .Where(t =>
                t.IsClass
                && !t.IsAbstract
                && !t.IsGenericTypeDefinition
                && t.Namespace is not null
                && (t.Namespace.EndsWith(".Repositories") || t.Namespace.EndsWith(".Services"))
                && (t.Name.EndsWith("Repository") || t.Name.EndsWith("Service"))
            );

        foreach (var impl in implementations)
        {
            var contract = impl.GetInterfaces().FirstOrDefault(i => i.Name == "I" + impl.Name);
            if (contract is not null)
                services.AddScoped(contract, impl);
        }

        services.AddScoped(
            typeof(QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories.IRepository<>),
            typeof(QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Repositories.Repository<>)
        );

        return services;
    }
}
