namespace ApplicationWorker.Models;

public record ApplicationSubmittedMessage
{
    public Guid ApplicationId { get; init; }
    public Guid IdempotencyKey { get; init; }
}
