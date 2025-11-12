using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class MedicationScheduleConfiguration : IEntityTypeConfiguration<MedicationSchedule>
    {
        public void Configure(EntityTypeBuilder<MedicationSchedule> builder)
        {
            builder.ToTable("MedicationSchedules");

            builder.HasKey(ms => ms.Id);

            builder.Property(ms => ms.PrescriptionId)
                .IsRequired();

            builder.Property(ms => ms.PrescriptionMedicineId)
                .IsRequired();

            builder.Property(ms => ms.ScheduleName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(ms => ms.StartDate)
                .IsRequired();

            builder.Property(ms => ms.EndDate);

            builder.Property(ms => ms.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ms => ms.CreatedAt)
                .IsRequired();

            builder.Property(ms => ms.UpdatedAt);

            // Relationships
            builder.HasOne(ms => ms.Prescription)
                .WithMany(up => up.MedicationSchedules)
                .HasForeignKey(ms => ms.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ms => ms.PrescriptionMedicine)
                .WithMany()
                .HasForeignKey(ms => ms.PrescriptionMedicineId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(ms => ms.ScheduleTimings)
                .WithOne(st => st.MedicationSchedule)
                .HasForeignKey(st => st.MedicationScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(ms => ms.IntakeLogs)
                .WithOne(il => il.MedicationSchedule)
                .HasForeignKey(il => il.MedicationScheduleId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

