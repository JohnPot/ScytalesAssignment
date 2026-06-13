using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database;

public class DrivingLicenceApplicationConfiguration : IEntityTypeConfiguration<DrivingLicenceApplication>
{
    public void Configure(EntityTypeBuilder<DrivingLicenceApplication> entity)
    {
        entity.ToTable("driving_licence_applications");

        entity.HasIndex(x => x.NationalId)
            .IsUnique()
            .HasDatabaseName("uq_national_id");

        entity.HasIndex(x => x.Email)
            .IsUnique()
            .HasDatabaseName("ux_applications_email");

        entity.ToTable(t =>
        {
            t.HasCheckConstraint(
                "chk_status",
                @"status IN (
                    'Pending',
                    'Submitted',
                    'ValidatingData',
                    'CrossCheckingRegistry',
                    'CheckingPhoto',
                    'RiskAssessment',
                    'PendingManualReview',
                    'Approved',
                    'Rejected',
                    'Failed'
                )");
        });

        entity.HasMany(x => x.Audits)
            .WithOne(x => x.Application)
            .HasForeignKey(x => x.ApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        entity.HasKey(x => x.Id);

        entity.Property(x => x.Id)
              .HasColumnName("id");

        entity.Property(x => x.FirstName)
              .IsRequired()
              .HasMaxLength(100)
              .HasColumnName("first_name");

        entity.Property(x => x.LastName)
              .IsRequired()
              .HasMaxLength(100)
              .HasColumnName("last_name");

        entity.Property(x => x.DateOfBirth)
              .IsRequired()
              .HasColumnName("date_of_birth");

        entity.Property(x => x.NationalId)
              .IsRequired()
              .HasMaxLength(50)
              .HasColumnName("national_id");

        entity.Property(x => x.Email)
              .IsRequired()
              .HasMaxLength(200)
              .HasColumnName("email");

        entity.Property(x => x.PhoneNumber)
              .IsRequired()
              .HasMaxLength(30)
              .HasColumnName("phone_number");

        entity.Property(x => x.Address)
              .IsRequired()
              .HasMaxLength(300)
              .HasColumnName("address");

        entity.Property(x => x.LicenceCategory)
              .IsRequired()
              .HasMaxLength(10)
              .HasColumnName("licence_category");

        entity.Property(x => x.Status)
              .IsRequired()
              .HasConversion<string>()
              .HasMaxLength(50)
              .HasColumnName("status");

        entity.Property(x => x.CreatedAt)
              .HasMaxLength(500)
              .HasColumnName("created_at");

        entity.Property(x => x.UpdatedAt)
              .HasMaxLength(500)
              .HasColumnName("updated_at");

        entity.Property(x => x.Photo)
              .HasColumnName("photo");

        entity.HasIndex(x => x.Email);
        entity.HasIndex(x => x.NationalId);
    }
}