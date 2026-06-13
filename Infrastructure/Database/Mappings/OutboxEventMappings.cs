using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> entity)
    {
        entity.ToTable("outbox_event");

        entity.HasIndex(x => new { x.Status, x.CreatedAt });

        entity.ToTable(t =>
        {
            t.HasCheckConstraint(
                "chk_status",
                @"status IN (
                    'Pending',
                    'Submitted'
                )");
        });

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
              .HasColumnName("id");

        entity.Property(x => x.Payload)
              .IsRequired()
              .HasMaxLength(100)
              .HasColumnName("payload");

        entity.Property(x => x.CreatedAt)
              .IsRequired()
              .HasMaxLength(100)
              .HasColumnName("created_at");

        entity.Property(x => x.Status)
              .IsRequired()
              .HasColumnName("status");

        entity.Property(x => x.ProcessedAt)
              .HasMaxLength(50)
              .HasColumnName("processed_at");

        entity.Property(x => x.Status)
              .IsRequired()
              .HasConversion<string>()
              .HasMaxLength(50)
              .HasColumnName("status");

        entity.Property(x => x.CreatedAt)
              .HasMaxLength(500)
              .HasColumnName("created_at");
    }
}