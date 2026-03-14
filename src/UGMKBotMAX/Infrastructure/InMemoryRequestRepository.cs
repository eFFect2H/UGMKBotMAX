using System.Collections.Concurrent;
using UGMKBotMAX.Application;
using UGMKBotMAX.Domain;

namespace UGMKBotMAX.Infrastructure;

public sealed class InMemoryRequestRepository : IRequestRepository
{
    private readonly ConcurrentDictionary<Guid, ServiceRequest> _storage = new();

    public Task AddAsync(ServiceRequest request, CancellationToken cancellationToken)
    {
        _storage.TryAdd(request.Id, request);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ServiceRequest>> GetOpenAsync(CancellationToken cancellationToken)
    {
        var result = _storage.Values.Where(x => x.Status is RequestStatus.Open or RequestStatus.Overdue).OrderBy(x => x.CreatedAt).ToList();
        return Task.FromResult<IReadOnlyList<ServiceRequest>>(result);
    }

    public Task<IReadOnlyList<ServiceRequest>> GetAllAsync(CancellationToken cancellationToken)
    {
        var result = _storage.Values.OrderBy(x => x.CreatedAt).ToList();
        return Task.FromResult<IReadOnlyList<ServiceRequest>>(result);
    }

    public Task<ServiceRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        _storage.TryGetValue(id, out var request);
        return Task.FromResult(request);
    }

    public Task UpdateAsync(ServiceRequest request, CancellationToken cancellationToken)
    {
        _storage[request.Id] = request;
        return Task.CompletedTask;
    }
}
