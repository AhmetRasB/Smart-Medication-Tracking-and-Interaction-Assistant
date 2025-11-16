using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class IntakeLogConfiguration : IEntityTypeConfiguration<IntakeLog>
    {
        public void Configure(EntityTypeBuilder<IntakeLog> builder)
        {
            builder.ToTable("IntakeLogs");

            builder.HasKey(il => il.Id);

            builder.Property(il => il.MedicationScheduleId)
                .IsRequired();

            builder.Property(il => il.UserId)
                .IsRequired();

            builder.Property(il => il.ScheduledTime)
                .IsRequired();

            builder.Property(il => il.TakenTime);

            builder.Property(il => il.IsTaken)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(il => il.IsSkipped)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(il => il.Notes)
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            builder.Property(il => il.CreatedAt)
                .IsRequired();

            builder.Property(il => il.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(il => il.DeletedAt);

            // Relationships
            builder.HasOne(il => il.MedicationSchedule)
                .WithMany(ms => ms.IntakeLogs)
                .HasForeignKey(il => il.MedicationScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(il => il.User)
                .WithMany()
                .HasForeignKey(il => il.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for better query performance
            builder.HasIndex(il => new { il.UserId, il.ScheduledTime });
            builder.HasIndex(il => new { il.MedicationScheduleId, il.ScheduledTime });
        }
    }
}

