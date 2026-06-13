using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public DbSet<DrivingLicenceApplication> Applications { get; set; }

    public DbSet<ApplicationAudit> ApplicationAudits { get; set; }
    public DbSet<OutboxEvent> OutboxEvents { get; set; }
    public DbSet<ProcessedEvent> ProcessedEvents { get; set; }
}