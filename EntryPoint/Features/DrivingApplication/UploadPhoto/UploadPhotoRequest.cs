namespace EntryPoint.Features.DrivingApplication.UploadPhoto;

public record UploadPhotoRequest
{
    public IFormFile Photo { get; init; }
}
