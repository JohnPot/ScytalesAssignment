using ApplicationWorker.Models;
using ApplicationWorker.Services;
using ApplicationWorker.Workflows;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;
using NATS.Client.Serializers.Json;
using NATS.Net;

namespace ApplicationWorker;

public class QueueWorker : BackgroundService
{
    private readonly ILogger<QueueWorker> _logger;
    private readonly NatsClient _nats;
    private readonly IServiceScopeFactory _scopeFactory;

    public QueueWorker(ILogger<QueueWorker> logger, NatsClient nats, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _nats = nats;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

        var js = _nats.CreateJetStreamContext();
        INatsJSConsumer consumer = null!;

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                consumer = await js.CreateConsumerAsync(
                        stream: "APPLICATIONS",
                        config: new ConsumerConfig("worker")
                        {
                            FilterSubject = "application.submitted",
                            AckPolicy = ConsumerConfigAckPolicy.Explicit,
                            DeliverPolicy = ConsumerConfigDeliverPolicy.All
                        },
                        cancellationToken: cancellationToken);
                _logger.LogInformation("Stream APPLICATIONS is ready");
                break;
            }
            catch
            {
                _logger.LogWarning("Stream not found yet. Retrying in 3 seconds...");
                await Task.Delay(3000, cancellationToken);
            }
        }

        var serializer = new NatsJsonSerializer<ApplicationSubmittedMessage>();
        var opts = new NatsJSConsumeOpts()
        {
            MaxMsgs = 10
        };

        while (!cancellationToken.IsCancellationRequested)
        {
            await foreach (var msg in consumer.ConsumeAsync<ApplicationSubmittedMessage>(serializer, opts, cancellationToken))
            {
                _logger.LogInformation("Processing {id}", msg.Data.ApplicationId);

                var applicationId = msg.Data.ApplicationId;

                using var scope = _scopeFactory.CreateScope();

                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var workflow = scope.ServiceProvider.GetRequiredService<ApplicationWorkflow>();
                var processedEvents = scope.ServiceProvider.GetRequiredService<CheckProcessedEvents>();

                await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    await processedEvents.AddIdempotencyKey(msg.Data.IdempotencyKey, cancellationToken);

                    await workflow.Execute(msg.Data.ApplicationId, cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    await msg.AckAsync();
                }
                catch (DbUpdateException ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    await msg.AckAsync();

                    _logger.LogInformation("Message with key {Key} already processed", msg.Data.IdempotencyKey);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);

                    _logger.LogError(ex, "Failed processing");

                    await msg.NakAsync();
                }
            }
        }
    }
}   
