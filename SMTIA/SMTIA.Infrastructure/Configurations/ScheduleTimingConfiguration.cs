using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class ScheduleTimingConfiguration : IEntityTypeConfiguration<ScheduleTiming>
    {
        public void Configure(EntityTypeBuilder<ScheduleTiming> builder)
        {
            builder.ToTable("ScheduleTimings");

            builder.HasKey(st => st.Id);

            builder.Property(st => st.MedicationScheduleId)
                .IsRequired();

            builder.Property(st => st.Time)
                .IsRequired()
                .HasConversion(
                    v => v.ToTimeSpan(),
                    v => TimeOnly.FromTimeSpan(v));

            builder.Property(st => st.Dosage)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(st => st.DosageUnit)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(st => st.DayOfWeek)
                .HasComment("0=Pazar, 1=Pazartesi, ..., 6=Cumartesi (null = her gün)");

            builder.Property(st => st.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(st => st.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasOne(st => st.MedicationSchedule)
                .WithMany(ms => ms.ScheduleTimings)
                .HasForeignKey(st => st.MedicationScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for better query performance
            builder.HasIndex(st => new { st.MedicationScheduleId, st.Time });
        }
    }
}

