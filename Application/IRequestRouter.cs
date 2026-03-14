using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application
{
    public interface IRequestRouter
    {
        string ResolveTargetChat(ServiceRequest request);
    }
}
