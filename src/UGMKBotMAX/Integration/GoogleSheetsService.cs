using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UGMKBotMAX.Application;
using UGMKBotMAX.Config;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Integration;

public sealed class GoogleSheetsService : IGoogleSheetsService
{
    private readonly GoogleSheetsSettings _settings;
    private readonly ILogger<GoogleSheetsService> _logger;

    public GoogleSheetsService(IOptions<GoogleSheetsSettings> settings, ILogger<GoogleSheetsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public Task AppendRequestAsync(ServiceRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Google Sheets append: SpreadsheetId={SpreadsheetId}, Sheet={SheetName}, Request={RequestId}",
            _settings.SpreadsheetId,
            _settings.SheetName,
            request.Id);

        // Здесь должна быть реальная интеграция через Google Sheets API:
        // 1. Инициализировать ServiceAccount через _settings.CredentialsJsonPath
        // 2. Выполнить append в диапазон _settings.SheetName
        // 3. Сохранить поля ТЗ: datetime, object, direction, description, contact, photo, telegram id.
        return Task.CompletedTask;
    }
}
