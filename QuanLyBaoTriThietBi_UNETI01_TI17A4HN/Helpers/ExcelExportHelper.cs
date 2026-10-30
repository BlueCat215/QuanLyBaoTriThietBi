#nullable enable
using System.Globalization;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace QuanLyBaoTriThietBi_UNETI01_TI17A4HN.Helpers;

/// <summary>
/// Cấu hình một cột xuất ra file Excel.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu của dòng</typeparam>
public class ExcelColumn<T>
{
    /// <summary>
    /// Tiêu đề cột (dòng Header)
    /// </summary>
    public string HeaderText { get; set; } = string.Empty;

    /// <summary>
    /// Hàm trích xuất giá trị cho ô từ đối tượng dữ liệu
    /// </summary>
    public Func<T, object?> ValueSelector { get; set; } = default!;

    /// <summary>
    /// Định dạng số / ngày tháng của Excel (vd: "#,##0", "dd/MM/yyyy", "dd/MM/yyyy HH:mm")
    /// </summary>
    public string? NumberFormat { get; set; }

    /// <summary>
    /// Căn lề ngang cho cột (mặc định: Trái)
    /// </summary>
    public XLAlignmentHorizontalValues Alignment { get; set; } = XLAlignmentHorizontalValues.Left;

    /// <summary>
    /// Độ rộng tùy chỉnh của cột (nếu không set sẽ tự động co giãn theo nội dung)
    /// </summary>
    public double? Width { get; set; }

    public ExcelColumn() { }

    public ExcelColumn(
        string headerText,
        Func<T, object?> valueSelector,
        string? numberFormat = null,
        XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left,
        double? width = null)
    {
        HeaderText = headerText;
        ValueSelector = valueSelector;
        NumberFormat = numberFormat;
        Alignment = alignment;
        Width = width;
    }
}

/// <summary>
/// Helper hạ tầng xuất dữ liệu ra file Excel (.xlsx) chuẩn hóa dùng chung cho toàn bộ dự án.
/// Sử dụng ClosedXML, hỗ trợ: Banner tiêu đề, Header nhận diện thương hiệu, Zebra striping,
/// căn chỉnh cột tự động, định dạng tiền tệ và ngày tháng chuẩn tiếng Việt.
/// </summary>
public static class ExcelExportHelper
{
    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    // Bảng màu chuẩn thiết kế đồ án (Dark Navy & Blue tone)
    private static readonly XLColor PrimaryHeaderBg = XLColor.FromArgb(24, 76, 120);     // #184C78
    private static readonly XLColor BannerBg = XLColor.FromArgb(15, 50, 80);             // #0F3250
    private static readonly XLColor MetadataBg = XLColor.FromArgb(240, 244, 248);       // #F0F4F8
    private static readonly XLColor ZebraOddBg = XLColor.FromArgb(248, 250, 252);        // #F8FAFC
    private static readonly XLColor BorderColor = XLColor.FromArgb(209, 213, 219);       // #D1D5DB

    /// <summary>
    /// Xuất danh sách dữ liệu ra mảng byte của file Excel (.xlsx).
    /// </summary>
    /// <typeparam name="T">Kiểu của bản ghi</typeparam>
    /// <param name="sheetName">Tên tab sheet trong file Excel</param>
    /// <param name="reportTitle">Tiêu đề lớn hiển thị trên cùng (vd: "DANH SÁCH THIẾT BỊ BẢO TRÌ")</param>
    /// <param name="data">Tập hợp dữ liệu cần xuất</param>
    /// <param name="columns">Danh sách cấu hình các cột</param>
    /// <param name="nguoiXuat">Họ tên hoặc vai trò người thực hiện xuất file</param>
    /// <param name="coCotStt">Có tự động chèn cột Số thứ tự (STT) ở đầu bảng không</param>
    /// <returns>Mảng byte của file Excel</returns>
    public static byte[] Export<T>(
        string sheetName,
        string reportTitle,
        IEnumerable<T> data,
        IReadOnlyList<ExcelColumn<T>> columns,
        string? nguoiXuat = null,
        bool coCotStt = true)
    {
        using var workbook = new XLWorkbook();
        var safeSheetName = string.IsNullOrWhiteSpace(sheetName) ? "Báo cáo" : sheetName.Trim();
        // Excel giới hạn tên Sheet tối đa 31 ký tự và không chứa ký tự cấm: \ / ? * : [ ]
        if (safeSheetName.Length > 30) safeSheetName = safeSheetName[..30];
        safeSheetName = safeSheetName.Replace("/", "-").Replace("\\", "-").Replace(":", "-")
                                     .Replace("?", "").Replace("*", "").Replace("[", "(").Replace("]", ")");

        var worksheet = workbook.Worksheets.Add(safeSheetName);
        worksheet.ShowGridLines = true;

        var totalCols = columns.Count + (coCotStt ? 1 : 0);
        if (totalCols == 0)
            throw new ArgumentException("Cần cấu hình ít nhất 1 cột để xuất Excel.", nameof(columns));

        var currentRow = 1;

        // 1. BANNER TIÊU ĐỀ LỚN
        var titleRange = worksheet.Range(currentRow, 1, currentRow, totalCols);
        titleRange.Merge();
        titleRange.Value = reportTitle.ToUpper(CultureInfo.GetCultureInfo("vi-VN"));
        titleRange.Style.Font.Bold = true;
        titleRange.Style.Font.FontSize = 15;
        titleRange.Style.Font.FontName = "Segoe UI";
        titleRange.Style.Font.FontColor = XLColor.White;
        titleRange.Style.Fill.BackgroundColor = BannerBg;
        titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Row(currentRow).Height = 36;
        currentRow++;

        // 2. METADATA: Thời gian xuất & Người xuất
        var metaRange = worksheet.Range(currentRow, 1, currentRow, totalCols);
        metaRange.Merge();
        var thoiGianStr = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
        metaRange.Value = string.IsNullOrWhiteSpace(nguoiXuat)
            ? $"Thời gian xuất báo cáo: {thoiGianStr}"
            : $"Thời gian xuất: {thoiGianStr} | Người xuất: {nguoiXuat}";
        metaRange.Style.Font.Italic = true;
        metaRange.Style.Font.FontSize = 9.5;
        metaRange.Style.Font.FontName = "Segoe UI";
        metaRange.Style.Font.FontColor = XLColor.FromArgb(75, 85, 99);
        metaRange.Style.Fill.BackgroundColor = MetadataBg;
        metaRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        metaRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        worksheet.Row(currentRow).Height = 22;
        currentRow += 2; // Cách 1 dòng trống cho thoáng

        // 3. DÒNG HEADER BẢNG DỮ LIỆU
        var headerRow = currentRow;
        var startCol = 1;

        if (coCotStt)
        {
            var cellStt = worksheet.Cell(headerRow, startCol);
            cellStt.Value = "STT";
            cellStt.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            startCol++;
        }

        for (var i = 0; i < columns.Count; i++)
        {
            var colDef = columns[i];
            var cell = worksheet.Cell(headerRow, startCol + i);
            cell.Value = colDef.HeaderText;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        var headerRange = worksheet.Range(headerRow, 1, headerRow, totalCols);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Font.FontSize = 10.5;
        headerRange.Style.Font.FontName = "Segoe UI";
        headerRange.Style.Font.FontColor = XLColor.White;
        headerRange.Style.Fill.BackgroundColor = PrimaryHeaderBg;
        headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        headerRange.Style.Alignment.WrapText = true;
        worksheet.Row(headerRow).Height = 28;

        currentRow++;

        // 4. ĐỔ DỮ LIỆU
        var stt = 1;
        var dataList = data as IList<T> ?? data.ToList();
        var firstDataRow = currentRow;

        foreach (var item in dataList)
        {
            var colIndex = 1;

            // STT
            if (coCotStt)
            {
                var sttCell = worksheet.Cell(currentRow, colIndex);
                sttCell.Value = stt;
                sttCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                colIndex++;
            }

            // Các cột dữ liệu
            for (var c = 0; c < columns.Count; c++)
            {
                var colDef = columns[c];
                var cell = worksheet.Cell(currentRow, colIndex + c);
                var rawValue = colDef.ValueSelector(item);

                DienGiaTriVaoO(cell, rawValue, colDef.NumberFormat);
                cell.Style.Alignment.Horizontal = colDef.Alignment;
            }

            // Zebra striping: nền xen kẽ để dễ đọc
            var rowRange = worksheet.Range(currentRow, 1, currentRow, totalCols);
            rowRange.Style.Font.FontName = "Segoe UI";
            rowRange.Style.Font.FontSize = 10;
            rowRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            if (stt % 2 == 0)
            {
                rowRange.Style.Fill.BackgroundColor = ZebraOddBg;
            }

            worksheet.Row(currentRow).Height = 22;
            stt++;
            currentRow++;
        }

        var lastDataRow = Math.Max(firstDataRow, currentRow - 1);

        // Kẻ khung toàn bộ bảng dữ liệu
        if (dataList.Count > 0)
        {
            var tableRange = worksheet.Range(headerRow, 1, lastDataRow, totalCols);
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            tableRange.Style.Border.InsideBorderColor = BorderColor;
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            tableRange.Style.Border.OutsideBorderColor = PrimaryHeaderBg;

            // Dòng tổng số lượng ở chân bảng
            var footerRow = currentRow;
            var footerRange = worksheet.Range(footerRow, 1, footerRow, totalCols);
            footerRange.Merge();
            footerRange.Value = $"Tổng cộng: {dataList.Count:#,##0} bản ghi";
            footerRange.Style.Font.Bold = true;
            footerRange.Style.Font.Italic = true;
            footerRange.Style.Font.FontSize = 10;
            footerRange.Style.Font.FontName = "Segoe UI";
            footerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            footerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            footerRange.Style.Fill.BackgroundColor = MetadataBg;
            footerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            footerRange.Style.Border.OutsideBorderColor = BorderColor;
            worksheet.Row(footerRow).Height = 24;
        }

        // 5. CĂN CHỈNH ĐỘ RỘNG CỘT TỰ ĐỘNG
        worksheet.Columns(1, totalCols).AdjustToContents();

        // Đảm bảo độ rộng tối thiểu và áp dụng Width tùy chỉnh
        var startCheckCol = 1;
        if (coCotStt)
        {
            worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 8);
            startCheckCol = 2;
        }

        for (var i = 0; i < columns.Count; i++)
        {
            var colNumber = startCheckCol + i;
            var customWidth = columns[i].Width;
            if (customWidth.HasValue)
            {
                worksheet.Column(colNumber).Width = customWidth.Value;
            }
            else
            {
                // Giới hạn độ rộng hợp lý
                var autoWidth = worksheet.Column(colNumber).Width;
                if (autoWidth < 12) worksheet.Column(colNumber).Width = 12;
                else if (autoWidth > 60) worksheet.Column(colNumber).Width = 60;
                else worksheet.Column(colNumber).Width = autoWidth + 2; // thêm padding nhẹ
            }
        }

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);
        return memoryStream.ToArray();
    }

    /// <summary>
    /// Điền giá trị vào ô và áp dụng đúng kiểu dữ liệu, định dạng số/ngày.
    /// </summary>
    private static void DienGiaTriVaoO(IXLCell cell, object? value, string? numberFormat)
    {
        if (value is null || value == DBNull.Value)
        {
            cell.Value = "—";
            return;
        }

        switch (value)
        {
            case DateTime dt:
                cell.Value = dt;
                cell.Style.DateFormat.Format = string.IsNullOrWhiteSpace(numberFormat) ? "dd/MM/yyyy" : numberFormat;
                break;

            case DateOnly d:
                cell.Value = d.ToDateTime(TimeOnly.MinValue);
                cell.Style.DateFormat.Format = string.IsNullOrWhiteSpace(numberFormat) ? "dd/MM/yyyy" : numberFormat;
                break;

            case decimal dec:
                cell.Value = dec;
                if (!string.IsNullOrWhiteSpace(numberFormat))
                    cell.Style.NumberFormat.Format = numberFormat;
                break;

            case double dbl:
                cell.Value = dbl;
                if (!string.IsNullOrWhiteSpace(numberFormat))
                    cell.Style.NumberFormat.Format = numberFormat;
                break;

            case int i:
                cell.Value = i;
                if (!string.IsNullOrWhiteSpace(numberFormat))
                    cell.Style.NumberFormat.Format = numberFormat;
                break;

            case long l:
                cell.Value = l;
                if (!string.IsNullOrWhiteSpace(numberFormat))
                    cell.Style.NumberFormat.Format = numberFormat;
                break;

            case bool b:
                cell.Value = b ? "Có" : "Không";
                break;

            default:
                cell.Value = value.ToString() ?? "—";
                break;
        }
    }
}
