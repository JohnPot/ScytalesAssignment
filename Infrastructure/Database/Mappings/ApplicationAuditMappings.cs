using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public class ApplicationAuditConfiguration : IEntityTypeConfiguration<ApplicationAudit>
{
    public void Configure(EntityTypeBuilder<ApplicationAudit> entity)
    {
        entity.ToTable("application_audits");

        entity.HasOne(x => x.Application)
            .WithMany(x => x.Audits)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
              .HasColumnName("id");

        entity.Property(x => x.ApplicationId)
              .HasMaxLength(500)
              .HasColumnName("application_id");

        entity.Property(x => x.EventType)
              .IsRequired()
              .HasConversion<string>()
              .HasMaxLength(50)
              .HasColumnName("event_type");

        entity.Property(x => x.FromStatus)
              .HasConversion<string>()
              .HasMaxLength(50)
              .HasColumnName("from_status");

        entity.Property(x => x.ToStatus)
              .HasConversion<string>()
              .HasMaxLength(50)
              .HasColumnName("to_status");

        entity.Property(x => x.Details)
              .HasMaxLength(500)
              .HasColumnName("details");

        entity.Property(x => x.CreatedAt)
              .HasMaxLength(500)
              .HasColumnName("created_at");

        entity.HasIndex(x => x.ApplicationId);
    }
}
