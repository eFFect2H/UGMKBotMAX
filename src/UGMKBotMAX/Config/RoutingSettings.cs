namespace UGMKBotMAX.Config;

public sealed class RoutingSettings
{
    public const string SectionName = "Routing";
    public long FallbackChatId { get; init; }
    public Dictionary<string, long> ChatRouting { get; init; } = new();
}
