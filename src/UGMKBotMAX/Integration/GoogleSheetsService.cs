using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using UGMKBotMAX.Application;
using UGMKBotMAX.Config;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Integration;

public sealed class GoogleSheetsService : IGoogleSheetsService
{
    private static readonly string[] Scopes = [SheetsService.Scope.Spreadsheets];

    private readonly GoogleSheetsSettings _settings;
    private readonly ILogger<GoogleSheetsService> _logger;
    private readonly Lazy<SheetsService> _sheetsService;

    public GoogleSheetsService(IOptions<GoogleSheetsSettings> settings, ILogger<GoogleSheetsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _sheetsService = new Lazy<SheetsService>(BuildClient, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    public async Task AppendRequestAsync(ServiceRequest request, CancellationToken cancellationToken)
    {
        var valueRange = new ValueRange
        {
            Values = new List<IList<object>>
            {
                new List<object>
                {
                    request.CreatedAt.ToString("O"),
                    request.FacilityType.ToString(),
                    request.BranchName ?? string.Empty,
                    request.Direction.ToString(),
                    request.Description,
                    request.ContactName,
                    request.ContactPhone,
                    request.PhotoUrl ?? string.Empty,
                    request.ApplicantChatId,
                    request.Id.ToString(),
                    request.Status.ToString()
                }
            }
        };

        var appendRequest = _sheetsService.Value.Spreadsheets.Values.Append(
            valueRange,
            _settings.SpreadsheetId,
            $"{_settings.SheetName}!A:K");

        appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.AppendRequest.ValueInputOptionEnum.USERENTERED;
        appendRequest.InsertDataOption = SpreadsheetsResource.ValuesResource.AppendRequest.InsertDataOptionEnum.INSERTROWS;

        await appendRequest.ExecuteAsync(cancellationToken);

        _logger.LogInformation("Request {RequestId} was appended to Google Sheets", request.Id);
    }

    private SheetsService BuildClient()
    {
        if (string.IsNullOrWhiteSpace(_settings.SpreadsheetId))
        {
            throw new InvalidOperationException("GoogleSheets:SpreadsheetId is not configured.");
        }

        if (!File.Exists(_settings.CredentialsJsonPath))
        {
            throw new FileNotFoundException("Google service account file was not found.", _settings.CredentialsJsonPath);
        }

        GoogleCredential credential;
        using (var stream = File.OpenRead(_settings.CredentialsJsonPath))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        return new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _settings.ApplicationName
        });
    }
}
