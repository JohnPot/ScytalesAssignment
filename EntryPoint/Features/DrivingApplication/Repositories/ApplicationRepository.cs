using EntryPoint.CommonMethods;
using EntryPoint.Features.DrivingApplication.Interfaces;
using EntryPoint.Features.DrivingApplication.Models.Requests;
using EntryPoint.Features.DrivingApplication.Models.Responses;
using Domain.Utilities;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace EntryPoint.Features.DrivingApplication.Repositories;

public class ApplicationRepository : IApplicationRepository
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationRepository> _logger;

    public ApplicationRepository(ApplicationDbContext context, ILogger<ApplicationRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<GetApplicationResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        //GET APPLICATION//
        var application = await _context.Applications
        .AsNoTracking()
        .Where(x => x.Id == id)
        .Select(x => new GetApplicationResponse()
        {
            ApplicationId = x.Id,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        })
        .FirstOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            _logger.LogError("application for id {id} was not found", id);
            return Result<GetApplicationResponse>.Failure("Application not found", ReturnStates.NotFound);
        }

        _logger.LogInformation("application for id {id} was found", id);
        return Result<GetApplicationResponse>.Success(application);
    }

    public async Task<Result<CreateApplicationResponse>> CreateApplication(CreateApplicationRequest request, CancellationToken cancellationToken)
    {
        //ADD APPLICATION//
        var entity = new DrivingLicenceApplication
        {
            Id = Guid.NewGuid(),
            FirstName = request.FirstName,
            LastName = request.LastName,
            DateOfBirth = request.DateOfBirth,
            NationalId = request.NationalId,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Address = request.Address,
            LicenceCategory = request.LicenceCategory,
            Status = ApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Applications.Add(entity);

        //ADD AUDIT//
        ApplicationAudit auditEntry = new()
        {
            Id = Guid.NewGuid(),
            ApplicationId = entity.Id,
            EventType = ApplicationEvents.ApplicationCreated,
            ToStatus = ApplicationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.ApplicationAudits.Add(auditEntry);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application created with id {id} ", entity.Id);

        return Result<CreateApplicationResponse>.Success(new CreateApplicationResponse() { ApplicationId = entity.Id, Status = ApplicationStatus.Pending });
    }

    public async Task<Result> AddPhotoAsync(Guid id, byte[] photo, CancellationToken cancellationToken)
    {
        //FIND APPLICATION//
        var application = await _context.Applications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new GetApplicationResponse()
            {
                ApplicationId = x.Id,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            _logger.LogError("Application with id {id} was not found", id);
            return Result.Failure("application not found", ReturnStates.NotFound);
        }

        if (application.Status != ApplicationStatus.Pending)
        {
            _logger.LogError("Application with id {id} is not in state {state}", id, Enum.GetName(ApplicationStatus.Pending));
            return Result.Failure("application not in pending state", ReturnStates.Conflict);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        //ADD APPLICATION PHOTO//
        int result = await _context.Applications
        .Where(x => x.Id == id && x.Photo != photo)
        .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.Photo, photo), cancellationToken);

        if (result == 0)
        {
            _logger.LogError("Couldn't upload photo for application with id {id}", id);
            return Result.Failure("Couldn't upload photo", ReturnStates.BadRequest);
        }

        //ADD AUDIT//
        _context.ApplicationAudits.Add(new ApplicationAudit
        {
            Id = Guid.NewGuid(),
            ApplicationId = id,
            EventType = ApplicationEvents.PhotoUploaded,
            Details = $"Size:{photo.Length}",
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Photo for application with id {id} was uploaded successfully", id);

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ApplicationSubmit(Guid id, CancellationToken cancellationToken)
    {
        //FIND APPLICATION//
        var application = await _context.Applications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new GetApplicationResponse()
            {
                ApplicationId = x.Id,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            _logger.LogError("Application with id {id} was not found", id);
            return Result.Failure("application not found", ReturnStates.NotFound);
        }

        if(application.Status != ApplicationStatus.Pending)
        {
            _logger.LogError("Application with id {id} is not in state {state}", id, Enum.GetName(ApplicationStatus.Pending));
            return Result.Failure("application not in pending state", ReturnStates.Conflict);
        }

        //CHECK FOR ANY EMPTY VALUES//
        var hasAnyEmptyValue = await _context.Applications
                    .AsNoTracking()
                    .Where(x => x.Id == id)
                    .AnyAsync(x =>
                        x.FirstName == null ||
                        x.LastName == null ||
                        x.Email == null ||
                        x.DateOfBirth == default ||
                        x.NationalId == null ||
                        x.PhoneNumber == null ||
                        x.Address == null ||
                        x.LicenceCategory == null ||
                        x.Photo == null || x.Photo.Length == 0 ||
                        x.CreatedAt == default);

        if (hasAnyEmptyValue)
        {
            _logger.LogError("Application with id {id} has empty values", id);
            return Result.Failure("application has empty values", ReturnStates.BadRequest);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        //SUBMIT APPLICATION//
        int result = await _context.Applications
       .Where(x => x.Id == id)
       .ExecuteUpdateAsync(setters => setters
       .SetProperty(x => x.Status, ApplicationStatus.Submitted), cancellationToken);

        if(result == 0)
        {
            _logger.LogError("Couldn't update the database, something went wrong");
            return Result.Failure("Couldn't update the database, something went wrong", ReturnStates.BadRequest);
        }

        //ADD OUTBOX EVENT//
        _context.OutboxEvents.Add(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            Payload = application.ApplicationId,
            Status = ApplicationOutboxStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        //ADD AUDIT//
        _context.ApplicationAudits.Add(new ApplicationAudit
        {
            Id = Guid.NewGuid(),
            ApplicationId = id,
            FromStatus = application.Status,
            ToStatus = ApplicationStatus.Submitted,
            EventType = ApplicationEvents.ApplicationSubmitted,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> UpdateStatusAsync(Guid id, ApplicationStatus status, string? reason, CancellationToken cancellationToken)
    {
        if(status is not(ApplicationStatus.Approved or ApplicationStatus.Rejected))
            return Result.Failure("Cannot change to any other state other than Approve/Reject", ReturnStates.BadRequest);

        //FIND APPLICATION//
        var application = await _context.Applications
            .Where(x => x.Id == id)
            .Select(x => new GetApplicationResponse()
            {
                ApplicationId = x.Id,
                Status = x.Status
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (application is null)
        {
            _logger.LogError("Application with id {id} was not found", id);
            return Result.Failure("Application not found", ReturnStates.NotFound);
        }

        if (application.Status != ApplicationStatus.PendingManualReview)
        {
            _logger.LogError("Application with id {id} is in state {wrongState} and not in state {rightState}", id, application.Status, Enum.GetName(ApplicationStatus.PendingManualReview));
            return Result.Failure("Only applications in status 'Manual Review' can be approved or rejected", ReturnStates.Conflict);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        //ADD AUDIT//
        _context.ApplicationAudits.Add(new ApplicationAudit
        {
            Id = Guid.NewGuid(),
            ApplicationId = id,
            FromStatus = application.Status,
            ToStatus = status,
            EventType = status switch
            {
                ApplicationStatus.Approved => ApplicationEvents.ValidationPassed,
                ApplicationStatus.Rejected => ApplicationEvents.ValidationFailed
            },
            Details = reason,
            CreatedAt = DateTime.UtcNow
        });

        //UPDATE APPLICATION STATUS//
        await _context.Applications
        .Where(x => x.Id == id)
        .ExecuteUpdateAsync(setters => setters
        .SetProperty(x => x.Status, status), cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Application with id {id} updated to status {status} successfully", id, Enum.GetName(status));

        await transaction.CommitAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result<List<GetApplicationHistoryResponse>>> ApplicationHistoryAsync(Guid id, CancellationToken cancellationToken)
    {
        //FIND APPLICATION HISTORY//
        var history = await _context.ApplicationAudits
        .AsNoTracking()
        .Where(x => x.ApplicationId == id)
        .OrderByDescending(x => x.CreatedAt)
        .Select(x => new GetApplicationHistoryResponse
        {
            Event = x.EventType,
            FromStatus = x.FromStatus,
            ToStatus = x.ToStatus,
            Details = x.Details,
            Timestamp = x.CreatedAt
        })
        .ToListAsync(cancellationToken);

        _logger.LogInformation("Application history for application with id {id} found", id);

        return Result<List<GetApplicationHistoryResponse>>.Success(history);
    }
}