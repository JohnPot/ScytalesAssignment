using EntryPoint.CommonMethods;
using Domain.Enums;
using EntryPoint.Features.DrivingApplication.GetApplication;
using EntryPoint.Features.DrivingApplication.CreateApplication;
using EntryPoint.Features.DrivingApplication.GetApplicationHistory;

namespace EntryPoint.Features.DrivingApplication.Interfaces;

public interface IApplicationRepository
{
    Task<Result<GetApplicationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<CreateApplicationResponse>> CreateApplication(CreateApplicationRequest application, CancellationToken cancellationToken);

    Task<Result> AddPhotoAsync(Guid id, byte[] photo, CancellationToken cancellationToken);
    Task<Result> ApplicationSubmit(Guid id, CancellationToken cancellationToken);

    Task<Result> UpdateStatusAsync(Guid id, ApplicationStatus status, string? reason, CancellationToken cancellationToken);

    Task<Result<List<GetApplicationHistoryResponse>>> ApplicationHistoryAsync(Guid id, CancellationToken cancellationToken);
}
