namespace EntryPoint.Features.DrivingApplication.Models.Requests;

public record RejectionRequest
{
    public string? reason { get; init; }
}
