using Domain.Utilities;

namespace EntryPoint.Features.DrivingApplication.Models.Responses;

public record GetApplicationHistoryResponse
{
    public ApplicationEvents Event { get; init; }
    public ApplicationStatus? FromStatus { get; init; }
    public ApplicationStatus? ToStatus { get; init; }
    public string? Details { get; init; }
    public DateTime Timestamp { get; set; }
}
