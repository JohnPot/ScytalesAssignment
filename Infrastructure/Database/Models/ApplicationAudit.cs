using Domain.Utilities;

namespace Infrastructure.Database;

public class ApplicationAudit
{
    public Guid Id { get; set; }

    public Guid ApplicationId { get; set; }

    public ApplicationStatus? FromStatus { get; set; }
    public ApplicationStatus? ToStatus { get; set; }

    public ApplicationEvents EventType { get; set; }

    public string? Details { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DrivingLicenceApplication Application { get; set; }
}
