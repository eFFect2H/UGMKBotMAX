using Microsoft.Extensions.Hosting;

namespace UGMKBotMAX.Application
{
    public sealed class BotWorker : BackgroundService
    {
        private readonly IMaxBotGateway _gateway;
        private readonly RequestOrchestrator _orchestrator;

        public BotWorker(IMaxBotGateway gateway, RequestOrchestrator orchestrator)
        {
            _gateway = gateway;
            _orchestrator = orchestrator;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _gateway.RunAsync(
                onNewRequest: async (input, ct) =>
                {
                    await _orchestrator.CreateRequestAsync(input, ct);
                },
                onCloseRequest: async (requestId, chatId, ct) =>
                {
                    await _orchestrator.CloseRequestAsync(requestId, chatId, ct);
                },
                onReportRequested: async (chatId, ct) =>
                {
                    var open = await _orchestrator.GetOpenRequestsAsync(ct);
                    await _gateway.SendOpenRequestsReportAsync(chatId, open, ct);
                },
                cancellationToken: stoppingToken);
        }
    }
}
