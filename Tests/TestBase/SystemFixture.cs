using IntegrationTests.Fixtures;

namespace IntegrationTests.TestBase;

public class SystemFixture : IAsyncLifetime
{
    public PostgresFixture Postgres { get; } = new();
    public NatsFixture Nats { get; } = new();

    public string ConnectionString => Postgres.ConnectionString;
    public string NatsConnectionString => Nats.ConnectionString;

    public async Task InitializeAsync()
    {
        await Postgres.InitializeAsync();
        await Nats.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Nats.DisposeAsync();
        await Postgres.DisposeAsync();
    }
}