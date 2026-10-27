// Họ tên: Phạm Việt Hoàn
// MSSV: 23103100207
// Nội dung: Module 3 - Helper vòng đời trạng thái Kế hoạch bảo trì (Workflow: Chờ thực hiện -> Đã phân công -> Đang thực hiện -> Chờ nghiệm thu -> Hoàn thành/Đã hủy, kiểm tra chuyển trạng thái hợp lệ, khóa chặn khi Hoàn thành/Đã hủy).
#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Models.Enums;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

public static class KeHoachWorkflowHelper
{
    /// <summary>
    /// Kiểm tra trạng thái đã kết thúc (Hoàn thành hoặc Đã hủy) - Khóa chặn mọi thao tác cập nhật/sửa/hủy.
    /// </summary>
    public static bool IsLocked(TrangThaiKeHoachEnum status) =>
        status is TrangThaiKeHoachEnum.HoanThanh or TrangThaiKeHoachEnum.DaHuy;

    /// <summary>
    /// Kiểm tra điều kiện chuyển đổi trạng thái hợp lệ theo đúng vòng đời quy định:
    /// Chờ thực hiện -> Đã phân công -> Đang thực hiện -> Chờ nghiệm thu -> Hoàn thành / Đã hủy.
    /// Chặn cập nhật khi đã ở Hoàn thành hoặc Đã hủy.
    /// </summary>
    public static bool CanTransitionTo(TrangThaiKeHoachEnum current, TrangThaiKeHoachEnum target)
    {
        if (current == target)
            return true;

        // Nếu đã ở trạng thái kết thúc thì khóa 100%, không cho phép nhảy sang bất kỳ trạng thái nào khác
        if (IsLocked(current))
            return false;

        return current switch
        {
            TrangThaiKeHoachEnum.ChoThucHien =>
                target is TrangThaiKeHoachEnum.DaPhanCong or TrangThaiKeHoachEnum.DaHuy,

            TrangThaiKeHoachEnum.DaPhanCong =>
                target is TrangThaiKeHoachEnum.DangThucHien
                    or TrangThaiKeHoachEnum.ChoThucHien // Trả về nếu rút phân công
                    or TrangThaiKeHoachEnum.DaHuy,

            TrangThaiKeHoachEnum.DangThucHien =>
                target is TrangThaiKeHoachEnum.ChoNghiemThu or TrangThaiKeHoachEnum.DaHuy,

            TrangThaiKeHoachEnum.ChoNghiemThu =>
                target is TrangThaiKeHoachEnum.HoanThanh // Nghiệm thu đạt
                    or TrangThaiKeHoachEnum.DangThucHien // Nghiệm thu chưa đạt, trả về kỹ thuật viên làm lại
                    or TrangThaiKeHoachEnum.DaHuy,

            _ => false,
        };
    }

    /// <summary>
    /// Lấy danh sách các trạng thái tiếp theo được phép chuyển tới từ trạng thái hiện tại.
    /// </summary>
    public static IReadOnlyList<TrangThaiKeHoachEnum> GetNextAllowedStatuses(TrangThaiKeHoachEnum current) =>
        Enum.GetValues<TrangThaiKeHoachEnum>()
            .Where(target => target != current && CanTransitionTo(current, target))
            .ToList();

    /// <summary>
    /// Tên tiếng Việt hiển thị cho từng trạng thái.
    /// </summary>
    public static string GetDisplayName(TrangThaiKeHoachEnum status) => status switch
    {
        TrangThaiKeHoachEnum.ChoThucHien => "Chờ thực hiện",
        TrangThaiKeHoachEnum.DaPhanCong => "Đã phân công",
        TrangThaiKeHoachEnum.DangThucHien => "Đang thực hiện",
        TrangThaiKeHoachEnum.ChoNghiemThu => "Chờ nghiệm thu",
        TrangThaiKeHoachEnum.HoanThanh => "Hoàn thành",
        TrangThaiKeHoachEnum.DaHuy => "Đã hủy",
        _ => "Không xác định",
    };

    /// <summary>
    /// Badge class cho hiển thị giao diện Bootstrap / Theme.
    /// </summary>
    public static string GetBadgeClass(TrangThaiKeHoachEnum status) => status switch
    {
        TrangThaiKeHoachEnum.ChoThucHien => "is-warning",
        TrangThaiKeHoachEnum.DaPhanCong => "is-info",
        TrangThaiKeHoachEnum.DangThucHien => "is-info is-strong",
        TrangThaiKeHoachEnum.ChoNghiemThu => "is-purple",
        TrangThaiKeHoachEnum.HoanThanh => "is-success",
        TrangThaiKeHoachEnum.DaHuy => "is-muted",
        _ => "is-light",
    };
}
