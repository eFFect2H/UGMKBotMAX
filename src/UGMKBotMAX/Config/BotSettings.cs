namespace UGMKBotMAX.Config;

public sealed class BotSettings
{
    public const string SectionName = "Bot";
    public string Token { get; init; } = string.Empty;
    public string Name { get; init; } = "Заявки УГМК";
    public List<long> AdminChatIds { get; init; } = new();
}
