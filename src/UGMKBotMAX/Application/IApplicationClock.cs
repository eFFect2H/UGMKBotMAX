namespace UGMKBotMAX.Application;

public interface IApplicationClock
{
    DateTimeOffset UtcNow { get; }
}
