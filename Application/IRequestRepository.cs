using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Application
{
    public interface IRequestRepository
    {
        Task AddAsync(ServiceRequest request, CancellationToken cancellationToken);
        Task<IReadOnlyList<ServiceRequest>> GetOpenAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken);
        Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
        Task UpdateAsync(ServiceRequest request, CancellationToken cancellationToken);
    }
}
