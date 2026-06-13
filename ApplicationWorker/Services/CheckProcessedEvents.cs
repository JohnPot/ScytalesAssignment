using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace ApplicationWorker.Services;

public class CheckProcessedEvents
{
    private readonly ApplicationDbContext _context;

    public CheckProcessedEvents(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddIdempotencyKey(Guid id, CancellationToken ct)
    {
        _context.ProcessedEvents.Add(new ProcessedEvent()
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = id
        });

        await _context.SaveChangesAsync(ct);

        return;
    }

    public async Task<bool> HasAnyDuplicates(Guid id, CancellationToken ct)
    {
        return await _context.ProcessedEvents
            .AnyAsync(x => x.IdempotencyKey == id);
    }
}