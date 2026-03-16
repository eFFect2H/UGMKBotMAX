namespace UGMKBotMAX.Config
{
    public sealed class RoutingSettings
    {
        public const string SectionName = "Routing";
        public string FallbackChat { get; init; } = "Техотдел | Чат";
        public Dictionary<string, string> ChatRouting { get; init; } = new();
    }
}
