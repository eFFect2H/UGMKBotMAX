using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UGMKBotMAX.Application;
using UGMKBotMAX.Config;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Infrastructure
{
    public sealed class MaxBotGateway : IMaxBotGateway
    {
        private readonly BotSettings _settings;
        private readonly ILogger<MaxBotGateway> _logger;

        public MaxBotGateway(IOptions<BotSettings> settings, ILogger<MaxBotGateway> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        public Task RunAsync(Func<NewRequestInput, CancellationToken, Task> onNewRequest, Func<Guid, long, CancellationToken, Task> onCloseRequest, Func<long, CancellationToken, Task> onReportRequested, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Max.Bot.SDK bootstrap. Token set: {HasToken}. Replace stub with real handlers for dialogs and callbacks.", !string.IsNullOrWhiteSpace(_settings.Token));

            // В этой точке подключается реальный клиент Max.Bot.SDK и регистрируются команды:
            // /start, /new, /close <id>, /report
            // а также сценарий диалога по шагам: объект -> направление -> описание -> фото -> контакты.
            return Task.CompletedTask;
        }

        public Task SendRequestToChatAsync(string chatAlias, ServiceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Routing request {RequestId} to chat {Chat}: {Direction} / {Facility}", request.Id, chatAlias, request.Direction, request.FacilityType);
            return Task.CompletedTask;
        }

        public Task NotifyApplicantRequestAcceptedAsync(ServiceRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notified applicant {ApplicantChatId} about request {RequestId}", request.ApplicantChatId, request.Id);
            return Task.CompletedTask;
        }

        public Task NotifyRequestClosedAsync(ServiceRequest request, long closedByChatId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Request {RequestId} closed by chat {ChatId}", request.Id, closedByChatId);
            return Task.CompletedTask;
        }

        public Task SendOpenRequestsReportAsync(long chatId, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Sent open requests report ({Count}) to chat {ChatId}", requests.Count, chatId);
            return Task.CompletedTask;
        }

        public Task SendReminderAsync(string chatAlias, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Reminder to {ChatAlias}: {Count} open requests", chatAlias, requests.Count);
            return Task.CompletedTask;
        }
    }
}
