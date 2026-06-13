namespace EntryPoint.Features.DrivingApplication.RejectApplication;

public record RejectApplicationRequest
{
    public string? reason { get; init; }
}
