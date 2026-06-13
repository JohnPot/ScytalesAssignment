using ApplicationWorker;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using NATS.Client.Core;
using NATS.Net;

namespace IntegrationTests.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _postgressConnectionString;
    private readonly string _natsConnectionString;

    public CustomWebApplicationFactory(string postgressConnectionString, string natsConnectionString)
    {
        _postgressConnectionString = postgressConnectionString;
        _natsConnectionString = natsConnectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((ctx, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Nats:Url"] = _natsConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_postgressConnectionString);
            });

            services.AddSingleton<NatsClient>(sp =>
            {
                var config = sp.GetRequiredService<IConfiguration>();

                string Url = config["Nats:Url"];

                return new NatsClient(new NatsOpts
                {
                    Url = Url
                });
            });

            services.AddHostedService<OutboxWorker>();
            services.AddHostedService<QueueWorker>();

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.Database.Migrate();
        });
    }
}