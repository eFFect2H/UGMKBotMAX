using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application;

public interface IGoogleSheetsService
{
    Task AppendRequestAsync(ServiceRequest request, CancellationToken cancellationToken);
}
