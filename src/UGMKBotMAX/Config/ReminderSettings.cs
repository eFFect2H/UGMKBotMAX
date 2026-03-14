namespace UGMKBotMAX.Config;

public sealed class ReminderSettings
{
    public const string SectionName = "Reminders";
    public int CheckPeriodMinutes { get; init; } = 1440;
    public int OverdueAfterHours { get; init; } = 24;
}
