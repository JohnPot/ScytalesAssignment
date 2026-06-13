using Domain.Utilities;

namespace ApplicationWorker.Dtos;

public record ApplicationDto
{
    public Guid Id { get; init; }
    public ApplicationStatus Status { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public DateTime DateOfBirth { get; init; }
    public string LicenceCategory { get; init; }
}
