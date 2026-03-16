using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UGMKBotMAX.Config;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application;

public sealed class DailyReminderWorker : BackgroundService
{
    private readonly IRequestRepository _repository;
    private readonly IRequestRouter _router;
    private readonly IMaxBotGateway _gateway;
    private readonly ReminderSettings _settings;
    private readonly IApplicationClock _clock;
    private readonly ILogger<DailyReminderWorker> _logger;

    public DailyReminderWorker(
        IRequestRepository repository,
        IRequestRouter router,
        IMaxBotGateway gateway,
        IOptions<ReminderSettings> settings,
        IApplicationClock clock,
        ILogger<DailyReminderWorker> logger)
    {
        _repository = repository;
        _router = router;
        _gateway = gateway;
        _settings = settings.Value;
        _clock = clock;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var delay = TimeSpan.FromMinutes(_settings.CheckPeriodMinutes);
            await Task.Delay(delay, stoppingToken);

            var open = await _repository.GetOpenAsync(stoppingToken);
            var grouped = open.GroupBy(_router.ResolveTargetChatId).ToList();
            foreach (var group in grouped)
            {
                var requests = group.ToList();
                foreach (var request in requests.Where(IsOverdue))
                {
                    request.MarkOverdue();
                    await _repository.UpdateAsync(request, stoppingToken);
                }

                await _gateway.SendReminderAsync(group.Key, requests, stoppingToken);
            }

            _logger.LogInformation("Sent {Count} reminder groups", grouped.Count);
        }
    }

    private bool IsOverdue(ServiceRequest request)
    {
        var age = _clock.UtcNow - request.CreatedAt;
        return age.TotalHours >= _settings.OverdueAfterHours;
    }
}
