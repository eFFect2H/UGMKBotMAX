namespace UGMKBotMAX.Domain;

public sealed class ServiceRequest
{
    public required Guid Id { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required FacilityType FacilityType { get; init; }
    public string? BranchName { get; init; }
    public required WorkDirection Direction { get; init; }
    public required string Description { get; init; }
    public string? PhotoUrl { get; init; }
    public required string ContactName { get; init; }
    public required string ContactPhone { get; init; }
    public required long ApplicantChatId { get; init; }
    public RequestStatus Status { get; private set; } = RequestStatus.Open;
    public DateTimeOffset? ClosedAt { get; private set; }

    public void Close(DateTimeOffset utcNow)
    {
        if (Status == RequestStatus.Closed)
        {
            return;
        }

        Status = RequestStatus.Closed;
        ClosedAt = utcNow;
    }

    public void MarkOverdue()
    {
        if (Status == RequestStatus.Open)
        {
            Status = RequestStatus.Overdue;
        }
    }
}
