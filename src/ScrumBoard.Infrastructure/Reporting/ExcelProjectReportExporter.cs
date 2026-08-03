using ClosedXML.Excel;
using ScrumBoard.Application.Abstractions.Reporting;
using ScrumBoard.Application.Reports;

namespace ScrumBoard.Infrastructure.Reporting;

public sealed class ExcelProjectReportExporter : IReportExporter
{
    public string Format => "xlsx";

    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public string FileExtension => "xlsx";

    public byte[] Export(ProjectReportDto report)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Proyecto");

        sheet.Cell("A1").Value = $"Reporte de proyecto: {report.ProjectName}";
        sheet.Range("A1:E1").Merge().Style.Font.SetBold().Font.SetFontSize(14);

        sheet.Cell("A2").Value = "Estado";
        sheet.Cell("B2").Value = ReportLabels.Status(report.Status);
        sheet.Cell("A3").Value = "Inicio";
        sheet.Cell("B3").Value = ReportLabels.Date(report.StartDate);
        sheet.Cell("A4").Value = "Fin previsto";
        sheet.Cell("B4").Value = ReportLabels.Date(report.PlannedEndDate);
        sheet.Cell("A5").Value = "Generado";
        sheet.Cell("B5").Value = ReportLabels.DateTimeUtc(report.GeneratedAtUtc);
        sheet.Range("A2:A5").Style.Font.SetBold();

        const int headerRow = 7;
        var headers = new[] { "Tarea", "Columna", "Responsable", "Prioridad", "Fecha de creación" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = sheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.SetBold();
            cell.Style.Fill.SetBackgroundColor(XLColor.LightGray);
            cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        }

        var currentRow = headerRow + 1;
        foreach (var row in report.Rows)
        {
            sheet.Cell(currentRow, 1).Value = row.TaskTitle;
            sheet.Cell(currentRow, 2).Value = row.ColumnName;
            sheet.Cell(currentRow, 3).Value = row.AssigneeName;
            sheet.Cell(currentRow, 4).Value = ReportLabels.Priority(row.Priority);
            sheet.Cell(currentRow, 5).Value = row.CreatedAtUtc;
            sheet.Cell(currentRow, 5).Style.DateFormat.Format = "dd/MM/yyyy";
            currentRow++;
        }

        if (report.Rows.Count > 0)
        {
            sheet.Range(headerRow, 1, currentRow - 1, headers.Length).SetAutoFilter();
            sheet.SheetView.FreezeRows(headerRow);
        }

        sheet.Columns(1, headers.Length).AdjustToContents(minWidth: 12d, maxWidth: 60d);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
