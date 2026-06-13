using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database;

public class ProcessedEventMappings : IEntityTypeConfiguration<ProcessedEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedEvent> entity)
    {
        entity.ToTable("processed_event");

        entity.HasKey(x => x.Id);

        entity.HasIndex(x => new { x.Id });
        entity.HasIndex(x => new { x.IdempotencyKey })
                .IsUnique();
    }
}