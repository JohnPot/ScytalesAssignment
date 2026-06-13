using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Networks;

namespace IntegrationTests.Fixtures;

public class NatsFixture : IAsyncLifetime
{
    private INetwork _network = null!;
    private IContainer _container = null!;

    public string ConnectionString { get; private set; } = null!;

    public INetwork Network => _network;

    public async Task InitializeAsync()
    {
        _network = new NetworkBuilder()
            .WithName($"test-network-{Guid.NewGuid()}")
            .Build();

        await _network.CreateAsync();

        _container = new ContainerBuilder()
            .WithImage("nats:latest")//.WithImage("nats:2.10")
            .WithCommand("-js")
            .WithPortBinding(4222, true)
            .WithWaitStrategy(Wait.ForUnixContainer()
                .UntilMessageIsLogged("Listening for client connections"))
            .WithOutputConsumer(Consume.RedirectStdoutAndStderrToConsole())
            .Build();


        await _container.StartAsync();

        var port = _container.GetMappedPublicPort(4222);
        ConnectionString = $"nats://localhost:{port}";
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await _network.DisposeAsync();
    }
}