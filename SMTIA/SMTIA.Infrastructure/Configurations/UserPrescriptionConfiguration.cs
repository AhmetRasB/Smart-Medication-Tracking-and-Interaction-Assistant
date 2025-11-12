using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class UserPrescriptionConfiguration : IEntityTypeConfiguration<UserPrescription>
    {
        public void Configure(EntityTypeBuilder<UserPrescription> builder)
        {
            builder.ToTable("UserPrescriptions");

            builder.HasKey(up => up.Id);

            builder.Property(up => up.UserId)
                .IsRequired();

            builder.Property(up => up.DoctorName)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(up => up.DoctorSpecialty)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(up => up.PrescriptionNumber)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(up => up.PrescriptionDate)
                .IsRequired();

            builder.Property(up => up.StartDate)
                .IsRequired();

            builder.Property(up => up.EndDate);

            builder.Property(up => up.Notes)
                .HasMaxLength(2000)
                .HasColumnType("varchar(2000)");

            builder.Property(up => up.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(up => up.CreatedAt)
                .IsRequired();

            builder.Property(up => up.UpdatedAt);

            // Relationships
            builder.HasOne(up => up.User)
                .WithMany()
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(up => up.PrescriptionMedicines)
                .WithOne(pm => pm.Prescription)
                .HasForeignKey(pm => pm.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(up => up.MedicationSchedules)
                .WithOne(ms => ms.Prescription)
                .HasForeignKey(ms => ms.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

