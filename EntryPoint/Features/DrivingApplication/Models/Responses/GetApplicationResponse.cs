using Domain.Utilities;

namespace EntryPoint.Features.DrivingApplication.Models.Responses;

public record GetApplicationResponse
{
    public Guid ApplicationId { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
