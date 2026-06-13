using ApplicationWorker;
using ApplicationWorker.Services;
using ApplicationWorker.Workflows;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using NATS.Net;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<QueueWorker>();
builder.Services.AddHostedService<OutboxWorker>();

builder.Services.AddSingleton<NatsClient>(_ =>
{
    var url = builder.Configuration.GetValue<string>("Nats:Url");
    return new NatsClient(url);
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<ApplicationWorkflow>();
builder.Services.AddScoped<CheckProcessedEvents>();

builder.Services.AddSingleton<NatsStartupInitializer>();

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
    { "message", new RenderedMessageColumnWriter() },
    { "messageTemplate", new MessageTemplateColumnWriter() },
    { "timeStamp", new TimestampColumnWriter() },
    { "exception", new ExceptionColumnWriter() },
    { "logEvent", new LogEventSerializedColumnWriter() }
};

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.PostgreSQL(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection"),
        tableName: "logs",
        columnOptions: columnWriters,
        needAutoCreateTable: true)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<NatsStartupInitializer>();
    bool isCreated = await initializer.Initialize();
}

host.Run();