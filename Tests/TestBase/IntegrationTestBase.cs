using IntegrationTests.Factories;

namespace IntegrationTests.TestBase;

public abstract class IntegrationTestBase : IClassFixture<SystemFixture>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly SystemFixture Fixture;

    protected IntegrationTestBase(SystemFixture fixture)
    {
        Fixture = fixture;

        Factory = new CustomWebApplicationFactory(
            fixture.Postgres.ConnectionString,
            fixture.NatsConnectionString);

        Client = Factory.CreateClient();
    }
}