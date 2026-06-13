using Domain.Utilities;

namespace EntryPoint.Features.DrivingApplication.Models.Responses;

public record CreateApplicationResponse
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }
}
