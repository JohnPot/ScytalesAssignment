namespace EntryPoint.Features.DrivingApplication.Models.Requests;

public record CreateApplicationPhotoRequest
{
    public IFormFile Photo { get; init; }
}
