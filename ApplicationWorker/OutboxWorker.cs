using ApplicationWorker.Models;
using Infrastructure.Database;
using Domain.Utilities;
using Microsoft.EntityFrameworkCore;
using NATS.Net;

namespace ApplicationWorker;

public class OutboxWorker : BackgroundService
{
    private readonly ILogger<OutboxWorker> _logger;
    private readonly NatsClient _nats;
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxWorker(ILogger<OutboxWorker> logger, NatsClient nats, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _nats = nats;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var js = _nats.CreateJetStreamContext();
        var cionn = _nats.Connection;

        while (!cancellationToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var events = await context.OutboxEvents
                .Where(x => x.Status == ApplicationOutboxStatus.Pending)
                .OrderBy(x => x.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);

            foreach (var ev in events)
            {
                try
                {
                    var message = new ApplicationSubmittedMessage
                    {
                        ApplicationId = ev.Payload,
                        IdempotencyKey = ev.Id
                    };

                    await js.PublishAsync("application.submitted", message, null, null, null, cancellationToken);

                    ev.Status = ApplicationOutboxStatus.Submitted;
                    ev.ProcessedAt = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    ev.Status = ApplicationOutboxStatus.Pending;

                    _logger.LogError(ex, "Failed to publish outbox event {Id}", ev.Id);

                    await Task.Delay(1000, cancellationToken);
                }
            }

            await context.SaveChangesAsync(cancellationToken);

            await Task.Delay(2000, cancellationToken);
        }
    }
}