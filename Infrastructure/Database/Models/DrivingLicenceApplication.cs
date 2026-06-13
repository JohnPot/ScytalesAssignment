using Domain.Utilities;

namespace Infrastructure.Database;

public class DrivingLicenceApplication
{
    public Guid Id { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    public DateTime DateOfBirth { get; set; }

    public string NationalId { get; set; }

    public string Email { get; set; }

    public string PhoneNumber { get; set; }

    public string Address { get; set; }

    public string LicenceCategory { get; set; }

    public ApplicationStatus Status { get; set; }

    public byte[]? Photo { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<ApplicationAudit> Audits { get; set; }
}
