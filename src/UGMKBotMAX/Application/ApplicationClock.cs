namespace UGMKBotMAX.Application;

public sealed class ApplicationClock : IApplicationClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
