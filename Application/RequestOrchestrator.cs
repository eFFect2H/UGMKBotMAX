using Microsoft.Extensions.Logging;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application
{
    public sealed class RequestOrchestrator
    {
        private readonly IRequestRepository _repository;
        private readonly IRequestRouter _router;
        private readonly IGoogleSheetsService _googleSheets;
        private readonly IMaxBotGateway _bot;
        private readonly IApplicationClock _clock;
        private readonly ILogger<RequestOrchestrator> _logger;

        public RequestOrchestrator(
            IRequestRepository repository,
            IRequestRouter router,
            IGoogleSheetsService googleSheets,
            IMaxBotGateway bot,
            IApplicationClock clock,
            ILogger<RequestOrchestrator> logger)
        {
            _repository = repository;
            _router = router;
            _googleSheets = googleSheets;
            _bot = bot;
            _clock = clock;
            _logger = logger;
        }

        public async Task<Guid> CreateRequestAsync(NewRequestInput input, CancellationToken cancellationToken)
        {
            var request = new ServiceRequest
            {
                Id = Guid.NewGuid(),
                CreatedAt = _clock.UtcNow,
                FacilityType = input.FacilityType,
                BranchName = input.BranchName,
                Direction = input.Direction,
                Description = input.Description,
                PhotoUrl = input.PhotoUrl,
                ContactName = input.ContactName,
                ContactPhone = input.ContactPhone,
                ApplicantChatId = input.ApplicantChatId
            };

            await _repository.AddAsync(request, cancellationToken);
            await _googleSheets.AppendRequestAsync(request, cancellationToken);

            var targetChat = _router.ResolveTargetChat(request);
            await _bot.SendRequestToChatAsync(targetChat, request, cancellationToken);
            await _bot.NotifyApplicantRequestAcceptedAsync(request, cancellationToken);

            _logger.LogInformation("Created request {RequestId} and routed to {TargetChat}", request.Id, targetChat);

            return request.Id;
        }

        public async Task<bool> CloseRequestAsync(Guid requestId, long closedByChatId, CancellationToken cancellationToken)
        {
            var request = await _repository.GetByIdAsync(requestId, cancellationToken);
            if (request is null)
            {
                return false;
            }

            request.Close(_clock.UtcNow);
            await _repository.UpdateAsync(request, cancellationToken);
            await _bot.NotifyRequestClosedAsync(request, closedByChatId, cancellationToken);

            return true;
        }

        public Task<IReadOnlyList<ServiceRequest>> GetOpenRequestsAsync(CancellationToken cancellationToken) =>
            _repository.GetOpenAsync(cancellationToken);
    }

    public sealed record NewRequestInput(
        FacilityType FacilityType,
        string? BranchName,
        WorkDirection Direction,
        string Description,
        string? PhotoUrl,
        string ContactName,
        string ContactPhone,
        long ApplicantChatId);
}
