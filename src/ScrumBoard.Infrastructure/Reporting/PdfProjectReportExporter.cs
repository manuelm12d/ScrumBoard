using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ScrumBoard.Application.Abstractions.Reporting;
using ScrumBoard.Application.Reports;

namespace ScrumBoard.Infrastructure.Reporting;

public sealed class PdfProjectReportExporter : IReportExporter
{
    public string Format => "pdf";

    public string ContentType => "application/pdf";

    public string FileExtension => "pdf";

    public byte[] Export(ProjectReportDto report)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(text => text.FontSize(10).FontFamily(Fonts.Calibri));

                page.Header().Element(header => ComposeHeader(header, report));
                page.Content().PaddingVertical(15).Element(content => ComposeTable(content, report));
                page.Footer().AlignCenter().Text(text =>
                {
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        }).GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, ProjectReportDto report)
    {
        container.Column(column =>
        {
            column.Item().Text($"Reporte de proyecto: {report.ProjectName}")
                .FontSize(16).SemiBold().FontColor(Colors.Blue.Darken2);

            if (!string.IsNullOrWhiteSpace(report.Description))
            {
                column.Item().PaddingTop(4).Text(report.Description!).FontSize(9).FontColor(Colors.Grey.Darken1);
            }

            column.Item().PaddingTop(6).Row(row =>
            {
                row.RelativeItem().Text($"Estado: {ReportLabels.Status(report.Status)}");
                row.RelativeItem().Text($"Inicio: {ReportLabels.Date(report.StartDate)}");
                row.RelativeItem().Text($"Fin previsto: {ReportLabels.Date(report.PlannedEndDate)}");
            });

            column.Item().Text($"Generado: {ReportLabels.DateTimeUtc(report.GeneratedAtUtc)}")
                .FontSize(9).FontColor(Colors.Grey.Darken1);

            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);
        });
    }

    private static void ComposeTable(IContainer container, ProjectReportDto report)
    {
        if (report.Rows.Count == 0)
        {
            container.Text("El proyecto no tiene tareas registradas.").Italic();
            return;
        }

        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(4);
                columns.RelativeColumn(2);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Tarea");
                HeaderCell(header.Cell(), "Columna");
                HeaderCell(header.Cell(), "Responsable");
                HeaderCell(header.Cell(), "Prioridad");
                HeaderCell(header.Cell(), "Creación");
            });

            foreach (var row in report.Rows)
            {
                BodyCell(table.Cell(), row.TaskTitle);
                BodyCell(table.Cell(), row.ColumnName);
                BodyCell(table.Cell(), row.AssigneeName);
                BodyCell(table.Cell(), ReportLabels.Priority(row.Priority));
                BodyCell(table.Cell(), row.CreatedAtUtc.ToString("dd/MM/yyyy"));
            }
        });
    }

    private static void HeaderCell(IContainer cell, string text) =>
        cell.Background(Colors.Grey.Lighten3).Padding(5).Text(text).SemiBold();

    private static void BodyCell(IContainer cell, string text) =>
        cell.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(text);
}
