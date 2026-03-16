using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application;

public interface IRequestRouter
{
    long ResolveTargetChatId(ServiceRequest request);
}
