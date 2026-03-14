using Microsoft.Extensions.Options;
using UGMKBotMAX.Application;
using UGMKBotMAX.Config;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Infrastructure
{
    public sealed class StaticRequestRouter : IRequestRouter
    {
        private readonly RoutingSettings _settings;

        public StaticRequestRouter(IOptions<RoutingSettings> settings)
        {
            _settings = settings.Value;
        }

        public string ResolveTargetChat(ServiceRequest request)
        {
            var key = $"{request.FacilityType}:{request.Direction}";
            if (_settings.ChatRouting.TryGetValue(key, out var value))
            {
                return value;
            }

            return _settings.FallbackChat;
        }
    }
}
