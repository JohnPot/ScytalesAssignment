namespace Infrastructure.Database;

public class ProcessedEvent
{
    public Guid Id { get; set; }

    public Guid IdempotencyKey { get; set; }
}