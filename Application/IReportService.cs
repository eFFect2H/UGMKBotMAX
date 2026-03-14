using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application
{
    public interface IReportService
    {
        Task<byte[]> BuildWeeklyExcelAsync(IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken);
        Task<byte[]> BuildWeeklyPdfAsync(IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken);
    }
}
