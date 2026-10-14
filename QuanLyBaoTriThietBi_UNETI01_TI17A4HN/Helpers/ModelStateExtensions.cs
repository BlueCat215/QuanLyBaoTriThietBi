#nullable enable
using Microsoft.AspNetCore.Mvc.ModelBinding;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Services;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

public static class ModelStateExtensions
{
    public static void AddServiceErrors(this ModelStateDictionary modelState, ServiceResult result)
    {
        foreach (var e in result.Errors)
            modelState.AddModelError(e.Field, e.Message);
    }
}
