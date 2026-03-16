using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application;

public interface IMaxBotGateway
{
    Task RunAsync(
        Func<NewRequestInput, CancellationToken, Task<Guid>> onNewRequest,
        Func<Guid, long, CancellationToken, Task<bool>> onCloseRequest,
        Func<long, CancellationToken, Task<IReadOnlyList<ServiceRequest>>> onReportRequested,
        CancellationToken cancellationToken);

    Task SendRequestToChatAsync(long chatId, ServiceRequest request, CancellationToken cancellationToken);
    Task NotifyApplicantRequestAcceptedAsync(ServiceRequest request, CancellationToken cancellationToken);
    Task NotifyRequestClosedAsync(ServiceRequest request, long closedByChatId, CancellationToken cancellationToken);
    Task SendOpenRequestsReportAsync(long chatId, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken);
    Task SendReminderAsync(long chatId, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken);
}
