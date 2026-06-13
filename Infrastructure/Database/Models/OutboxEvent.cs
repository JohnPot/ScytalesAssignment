using Domain.Utilities;

namespace Infrastructure.Database;

public class OutboxEvent
{
    public Guid Id { get; set; }

    public Guid Payload { get; set; }

    public DateTime CreatedAt { get; set; }

    public ApplicationOutboxStatus Status { get; set; }

    public DateTime? ProcessedAt { get; set; }
}