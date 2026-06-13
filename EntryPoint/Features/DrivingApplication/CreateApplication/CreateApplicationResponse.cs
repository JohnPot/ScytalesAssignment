using Domain.Enums;

namespace EntryPoint.Features.DrivingApplication.CreateApplication;

public record CreateApplicationResponse
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }
}
