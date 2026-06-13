using Testcontainers.PostgreSql;

namespace IntegrationTests.Fixtures;

public class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer _container = null!;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        _container = new PostgreSqlBuilder()
            .WithDatabase("test_db")
            .WithUsername("postgres")
            .WithPassword("postgres")
        .Build();

        await _container.StartAsync(_cts.Token);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}