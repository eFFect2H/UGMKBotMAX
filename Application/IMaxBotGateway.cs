using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application
{
    public interface IMaxBotGateway
    {
        Task RunAsync(Func<NewRequestInput, CancellationToken, Task> onNewRequest, Func<Guid, long, CancellationToken, Task> onCloseRequest, Func<long, CancellationToken, Task> onReportRequested, CancellationToken cancellationToken);
        Task SendRequestToChatAsync(string chatAlias, ServiceRequest request, CancellationToken cancellationToken);
        Task NotifyApplicantRequestAcceptedAsync(ServiceRequest request, CancellationToken cancellationToken);
        Task NotifyRequestClosedAsync(ServiceRequest request, long closedByChatId, CancellationToken cancellationToken);
        Task SendOpenRequestsReportAsync(long chatId, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken);
        Task SendReminderAsync(string chatAlias, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken);
    }
}
