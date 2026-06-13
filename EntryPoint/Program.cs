using EntryPoint.CommonMethods;
using EntryPoint.Middleware;
using FluentValidation.AspNetCore;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var columnWriters = new Dictionary<string, ColumnWriterBase>
{
    { "level", new LevelColumnWriter(true, NpgsqlDbType.Text) },
    { "message", new RenderedMessageColumnWriter() },
    { "messageTemplate", new MessageTemplateColumnWriter() },
    { "timeStamp", new TimestampColumnWriter() },
    { "exception", new ExceptionColumnWriter() },
    { "logEvent", new LogEventSerializedColumnWriter() }
};

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.PostgreSQL(
            connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
            tableName: "logs",
            columnOptions: columnWriters,
            needAutoCreateTable: true);
});

AppSettings.Configuration = builder.Configuration;

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;

        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "EntryPoint App", Version = "v1" });

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
}

builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddDependency();
builder.Services.AddValidators();

builder.WebHost.UseUrls("http://0.0.0.0:80");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EntryPoint App v1");
    });
}

app.UseMiddleware<Middleware>();

app.MapControllers();

//MIGRATIONS//
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    db.Database.Migrate();
}

app.Run();

public partial class Program { }