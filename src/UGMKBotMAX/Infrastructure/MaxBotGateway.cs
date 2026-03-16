using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UGMKBotMAX.Application;
using UGMKBotMAX.Config;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Infrastructure;

public sealed class MaxBotGateway : IMaxBotGateway
{
    private readonly BotSettings _settings;
    private readonly ILogger<MaxBotGateway> _logger;
    private readonly ConcurrentDictionary<long, DraftRequestSession> _drafts = new();

    private Func<NewRequestInput, CancellationToken, Task<Guid>>? _onNewRequest;
    private Func<Guid, long, CancellationToken, Task<bool>>? _onCloseRequest;
    private Func<long, CancellationToken, Task<IReadOnlyList<ServiceRequest>>>? _onReportRequested;

    public MaxBotGateway(IOptions<BotSettings> settings, ILogger<MaxBotGateway> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task RunAsync(
        Func<NewRequestInput, CancellationToken, Task<Guid>> onNewRequest,
        Func<Guid, long, CancellationToken, Task<bool>> onCloseRequest,
        Func<long, CancellationToken, Task<IReadOnlyList<ServiceRequest>>> onReportRequested,
        CancellationToken cancellationToken)
    {
        _onNewRequest = onNewRequest;
        _onCloseRequest = onCloseRequest;
        _onReportRequested = onReportRequested;

        _logger.LogInformation(
            "Max.Bot gateway initialized. Bot token provided={HasToken}. Dialog workflow and command handlers are active.",
            !string.IsNullOrWhiteSpace(_settings.Token));

        return Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
    }

    public async Task HandleIncomingMessageAsync(long chatId, string text, CancellationToken cancellationToken)
    {
        text = text.Trim();

        if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            await SendMessageAsync(chatId, "Добро пожаловать в сервис заявок УГМК. Для создания заявки используйте /new.", cancellationToken);
            return;
        }

        if (text.Equals("/new", StringComparison.OrdinalIgnoreCase))
        {
            _drafts[chatId] = new DraftRequestSession();
            await SendMessageAsync(chatId, "Выберите объект: 1) Корпус 1 2) Корпус 2 3) Филиалы", cancellationToken);
            return;
        }

        if (text.StartsWith("/close", StringComparison.OrdinalIgnoreCase))
        {
            if (_onCloseRequest is null)
            {
                return;
            }

            var split = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2 || !Guid.TryParse(split[1], out var requestId))
            {
                await SendMessageAsync(chatId, "Формат: /close <guid>", cancellationToken);
                return;
            }

            var closed = await _onCloseRequest(requestId, chatId, cancellationToken);
            await SendMessageAsync(chatId, closed ? "Заявка закрыта." : "Заявка не найдена.", cancellationToken);
            return;
        }

        if (text.Equals("/report", StringComparison.OrdinalIgnoreCase) || text.Equals("/отчет", StringComparison.OrdinalIgnoreCase))
        {
            if (_onReportRequested is null)
            {
                return;
            }

            var report = await _onReportRequested(chatId, cancellationToken);
            await SendOpenRequestsReportAsync(chatId, report, cancellationToken);
            return;
        }

        if (_drafts.TryGetValue(chatId, out var draft))
        {
            await ProcessDraftStepAsync(chatId, draft, text, cancellationToken);
        }
    }

    public Task SendRequestToChatAsync(long chatId, ServiceRequest request, CancellationToken cancellationToken) =>
        SendMessageAsync(chatId, BuildDispatcherCard(request), cancellationToken);

    public Task NotifyApplicantRequestAcceptedAsync(ServiceRequest request, CancellationToken cancellationToken) =>
        SendMessageAsync(request.ApplicantChatId, $"Заявка принята. Номер: {request.Id}", cancellationToken);

    public Task NotifyRequestClosedAsync(ServiceRequest request, long closedByChatId, CancellationToken cancellationToken) =>
        SendMessageAsync(request.ApplicantChatId, $"Заявка {request.Id} закрыта. Исполнитель: {closedByChatId}", cancellationToken);

    public Task SendOpenRequestsReportAsync(long chatId, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
    {
        var message = requests.Count == 0
            ? "Открытых заявок нет."
            : "Открытые заявки:\n" + string.Join("\n", requests.Select(r => $"- {r.Id} | {r.Direction} | {r.Status} | {r.CreatedAt:dd.MM HH:mm}"));

        return SendMessageAsync(chatId, message, cancellationToken);
    }

    public Task SendReminderAsync(long chatId, IReadOnlyList<ServiceRequest> requests, CancellationToken cancellationToken)
    {
        var overdue = requests.Count(r => r.Status == RequestStatus.Overdue);
        var text = $"Напоминание: открытых заявок {requests.Count}, просроченных {overdue}. Команда отчета: /report";
        return SendMessageAsync(chatId, text, cancellationToken);
    }

    private async Task ProcessDraftStepAsync(long chatId, DraftRequestSession draft, string text, CancellationToken cancellationToken)
    {
        switch (draft.Step)
        {
            case DraftStep.Facility:
                if (!TryParseFacility(text, out var facilityType, out var branchName))
                {
                    await SendMessageAsync(chatId, "Некорректный объект. Введите: Корпус 1 / Корпус 2 / Филиалы:<название>", cancellationToken);
                    return;
                }

                draft.FacilityType = facilityType;
                draft.BranchName = branchName;
                draft.Step = DraftStep.Direction;
                await SendMessageAsync(chatId, "Выберите направление: Электрика / Вода и отопление / Вентиляция / Отделка/строительство / Прочее", cancellationToken);
                break;

            case DraftStep.Direction:
                if (!TryParseDirection(text, out var direction))
                {
                    await SendMessageAsync(chatId, "Некорректное направление.", cancellationToken);
                    return;
                }

                draft.Direction = direction;
                draft.Step = DraftStep.Description;
                await SendMessageAsync(chatId, "Опишите проблему.", cancellationToken);
                break;

            case DraftStep.Description:
                draft.Description = text;
                draft.Step = DraftStep.Photo;
                await SendMessageAsync(chatId, "Пришлите ссылку на фото или '-' если без фото.", cancellationToken);
                break;

            case DraftStep.Photo:
                draft.PhotoUrl = text == "-" ? null : text;
                draft.Step = DraftStep.ContactName;
                await SendMessageAsync(chatId, "Введите ФИО заявителя.", cancellationToken);
                break;

            case DraftStep.ContactName:
                draft.ContactName = text;
                draft.Step = DraftStep.ContactPhone;
                await SendMessageAsync(chatId, "Введите телефон заявителя.", cancellationToken);
                break;

            case DraftStep.ContactPhone:
                if (_onNewRequest is null || draft.FacilityType is null || draft.Direction is null || draft.Description is null || draft.ContactName is null)
                {
                    await SendMessageAsync(chatId, "Сервис временно недоступен.", cancellationToken);
                    _drafts.TryRemove(chatId, out _);
                    return;
                }

                var input = new NewRequestInput(
                    draft.FacilityType.Value,
                    draft.BranchName,
                    draft.Direction.Value,
                    draft.Description,
                    draft.PhotoUrl,
                    draft.ContactName,
                    text,
                    chatId);

                var requestId = await _onNewRequest(input, cancellationToken);
                _drafts.TryRemove(chatId, out _);
                await SendMessageAsync(chatId, $"Заявка создана: {requestId}", cancellationToken);
                break;
        }
    }

    private Task SendMessageAsync(long chatId, string text, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Send message to {ChatId}: {Text}", chatId, text);
        return Task.CompletedTask;
    }

    private static bool TryParseFacility(string input, out FacilityType facilityType, out string? branchName)
    {
        branchName = null;

        if (input.Equals("Корпус 1", StringComparison.OrdinalIgnoreCase) || input == "1")
        {
            facilityType = FacilityType.Building1;
            return true;
        }

        if (input.Equals("Корпус 2", StringComparison.OrdinalIgnoreCase) || input == "2")
        {
            facilityType = FacilityType.Building2;
            return true;
        }

        if (input.StartsWith("Филиалы", StringComparison.OrdinalIgnoreCase) || input == "3")
        {
            facilityType = FacilityType.Branch;
            var split = input.Split(':', 2, StringSplitOptions.TrimEntries);
            branchName = split.Length == 2 ? split[1] : null;
            return true;
        }

        facilityType = default;
        return false;
    }

    private static bool TryParseDirection(string input, out WorkDirection direction)
    {
        direction = input.ToLowerInvariant() switch
        {
            "электрика" => WorkDirection.Electrical,
            "вода и отопление" => WorkDirection.WaterAndHeating,
            "вентиляция" => WorkDirection.Ventilation,
            "отделка/строительство" => WorkDirection.Construction,
            "прочее" => WorkDirection.Other,
            _ => default
        };

        return input is "Электрика" or "Вода и отопление" or "Вентиляция" or "Отделка/строительство" or "Прочее";
    }

    private static string BuildDispatcherCard(ServiceRequest request)
    {
        return $"Новая заявка {request.Id}\n" +
               $"Объект: {request.FacilityType} {(request.BranchName ?? string.Empty)}\n" +
               $"Направление: {request.Direction}\n" +
               $"Описание: {request.Description}\n" +
               $"Контакт: {request.ContactName}, {request.ContactPhone}\n" +
               $"Фото: {(string.IsNullOrWhiteSpace(request.PhotoUrl) ? "нет" : request.PhotoUrl)}\n" +
               $"Создана: {request.CreatedAt:dd.MM.yyyy HH:mm}";
    }

    private sealed class DraftRequestSession
    {
        public DraftStep Step { get; set; } = DraftStep.Facility;
        public FacilityType? FacilityType { get; set; }
        public string? BranchName { get; set; }
        public WorkDirection? Direction { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }
        public string? ContactName { get; set; }
    }

    private enum DraftStep
    {
        Facility,
        Direction,
        Description,
        Photo,
        ContactName,
        ContactPhone
    }
}
