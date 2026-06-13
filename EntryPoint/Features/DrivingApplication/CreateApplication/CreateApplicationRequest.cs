namespace EntryPoint.Features.DrivingApplication.CreateApplication;

public record CreateApplicationRequest
{
    public string FirstName { get; init; }

    public string LastName { get; init; }

    public DateTime DateOfBirth { get; init; }

    public string NationalId { get; init; }

    public string Email { get; init; }

    public string PhoneNumber { get; init; }

    public string Address { get; init; }

    public string LicenceCategory { get; init; }
}
