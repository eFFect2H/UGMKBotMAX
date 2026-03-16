namespace UGMKBotMAX.Config;

public sealed class GoogleSheetsSettings
{
    public const string SectionName = "GoogleSheets";
    public string SpreadsheetId { get; init; } = string.Empty;
    public string SheetName { get; init; } = "Заявки";
    public string CredentialsJsonPath { get; init; } = "./google-credentials.json";
    public string ApplicationName { get; init; } = "UGMKBotMAX";
}
