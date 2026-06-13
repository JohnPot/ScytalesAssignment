using ApplicationWorker.Dtos;
using Domain.Transitions;
using Infrastructure.Database;
using Domain.Utilities;
using Microsoft.EntityFrameworkCore;

namespace ApplicationWorker.Services;

public class ApplicationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(ApplicationDbContext context, ILogger<ApplicationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApplicationDto> Get(Guid id, CancellationToken ct)
    {
        return await _context.Applications
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ApplicationDto()
            {
                Id = x.Id,
                Status = x.Status,
                UpdatedAt = x.UpdatedAt,
                DateOfBirth = x.DateOfBirth,
                LicenceCategory = x.LicenceCategory
            })
            .FirstAsync(ct);
    }

    public async Task<ApplicationStatus> MoveToAndReturnState(Guid applicationId, ApplicationStatus from, ApplicationStatus to, CancellationToken ct)
    {
        if (!TransitionValidator.CanTransition(from, to))
        {
            _logger.LogError("Invalid transition {From} -> {To} for {Id}", from, to, applicationId);

            return from;
        }

        await _context.Applications
            .Where(x => x.Id == applicationId)
            .ExecuteUpdateAsync(setters => setters
            .SetProperty(x => x.Status, to)
            .SetProperty(x => x.UpdatedAt, DateTime.UtcNow), ct);

        ApplicationEvents eventType = TransitionLogic.returnEventBasedOnApplicationStatus(to);
        _context.ApplicationAudits.Add(new ApplicationAudit
        {
            Id = Guid.NewGuid(),
            ApplicationId = applicationId,
            FromStatus = from,
            ToStatus = to,
            EventType = eventType,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Transitioned {From} -> {To} for {Id}", from, to, applicationId);

        return to;
    }
}