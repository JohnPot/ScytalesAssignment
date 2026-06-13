using NATS.Client.JetStream.Models;
using NATS.Net;

namespace ApplicationWorker.Services;

public class NatsStartupInitializer
{
    private readonly NatsClient _nats;

    public NatsStartupInitializer(NatsClient nats)
    {
        _nats = nats;
    }

    public async Task<bool> Initialize()
    {
        var js = _nats.CreateJetStreamContext();
        try
        {
            await js.CreateStreamAsync(new StreamConfig
            {
                Name = "APPLICATIONS",
                Subjects = new[] { "application.*" }
            });

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}