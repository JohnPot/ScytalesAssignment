using EntryPoint.Features.DrivingApplication.Models.Requests;
using FluentValidation;

namespace EntryPoint.Features.DrivingApplication.Validations;

public class DrivingLicenceApplicationValidator : AbstractValidator<CreateApplicationRequest>
{
    public DrivingLicenceApplicationValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("Firstname must be provided");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Lastname must be provided");

        RuleFor(x => x.DateOfBirth)
            .NotEmpty()
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .WithMessage("Applicant must be at least 18 years old.");

        RuleFor(x => x.NationalId)
            .NotEmpty()
            .WithMessage("National ID must be provided");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email must be valid");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches("^[0-9]+$")
            .WithMessage("Phone number must be provided");

        RuleFor(x => x.Address)
            .NotEmpty()
            .WithMessage("Address must be provided");

        RuleFor(x => x.LicenceCategory)
            .NotEmpty()
            .WithMessage("Licence category must be provided");
    }
}
