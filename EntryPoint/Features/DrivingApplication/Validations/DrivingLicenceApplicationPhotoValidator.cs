using EntryPoint.Features.DrivingApplication.Models.Requests;
using FluentValidation;

namespace EntryPoint.Features.DrivingApplication.Validations;

public class DrivingLicenceApplicationPhotoValidator : AbstractValidator<CreateApplicationPhotoRequest>
{
    public DrivingLicenceApplicationPhotoValidator()
    {
        RuleFor(x => x.Photo)
            .NotEmpty()
            .WithMessage("Photo must be provided");

        RuleFor(x => x.Photo.ContentType)
            .Must(BeSupportedType)
            .When(x => x.Photo != null)
            .WithMessage("File type is unsupported");

        RuleFor(x => x.Photo.Length)
            .GreaterThan(10 * 1024)
            .When(x => x.Photo != null)
            .WithMessage("File size is too small");
    }

    private bool BeSupportedType(string contentType)
    {
        return contentType == "image/jpeg"
            || contentType == "image/png";
    }
}
