using System.Text;
using UGMKBotMAX.Application;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Integration;

public sealed class ReportService : IReportService
{
    public Task<byte[]> BuildWeeklyExcelAsync(IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
    {
        // Заглушка CSV как совместимый формат для Excel.
        var sb = new StringBuilder();
        sb.AppendLine("CreatedAt;Facility;Direction;Description;Contact;Phone;Status");

        foreach (var request in requests)
        {
            sb.AppendLine($"{request.CreatedAt:O};{request.FacilityType};{request.Direction};{Escape(request.Description)};{Escape(request.ContactName)};{Escape(request.ContactPhone)};{request.Status}");
        }

        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }

    public Task<byte[]> BuildWeeklyPdfAsync(IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
    {
        // Заглушка: plain text bytes. Подменяется на QuestPDF документ в production.
        var content = $"Weekly report generated at {DateTimeOffset.UtcNow:O}. Rows: {requests.Count}";
        return Task.FromResult(Encoding.UTF8.GetBytes(content));
    }

    private static string Escape(string value) => value.Replace(";", ",");
}
