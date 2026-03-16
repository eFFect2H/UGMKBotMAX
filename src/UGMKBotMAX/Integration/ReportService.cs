using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UGMKBotMAX.Application;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Integration;

public sealed class ReportService : IReportService
{
    public Task<byte[]> BuildWeeklyExcelAsync(IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("WeeklyReport");

        var headers = new[] { "CreatedAt", "Facility", "Branch", "Direction", "Description", "Contact", "Phone", "Status", "RequestId" };
        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (var row = 0; row < requests.Count; row++)
        {
            var request = requests[row];
            var line = row + 2;

            worksheet.Cell(line, 1).Value = request.CreatedAt.UtcDateTime;
            worksheet.Cell(line, 2).Value = request.FacilityType.ToString();
            worksheet.Cell(line, 3).Value = request.BranchName;
            worksheet.Cell(line, 4).Value = request.Direction.ToString();
            worksheet.Cell(line, 5).Value = request.Description;
            worksheet.Cell(line, 6).Value = request.ContactName;
            worksheet.Cell(line, 7).Value = request.ContactPhone;
            worksheet.Cell(line, 8).Value = request.Status.ToString();
            worksheet.Cell(line, 9).Value = request.Id.ToString();
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }

    public Task<byte[]> BuildWeeklyPdfAsync(IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Text($"UGMK weekly report ({DateTimeOffset.Now:dd.MM.yyyy})").Bold().FontSize(16);

                page.Content().Column(column =>
                {
                    foreach (var request in requests)
                    {
                        column.Item().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(6).PaddingTop(6).Text(
                            $"{request.CreatedAt:dd.MM HH:mm} | {request.FacilityType} | {request.Direction} | {request.Status}\n" +
                            $"{request.Description}\n" +
                            $"{request.ContactName} ({request.ContactPhone}) | #{request.Id}");
                    }
                });
            });
        });

        return Task.FromResult(document.GeneratePdf());
    }
}
