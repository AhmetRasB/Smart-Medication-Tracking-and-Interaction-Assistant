using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class PrescriptionMedicineConfiguration : IEntityTypeConfiguration<PrescriptionMedicine>
    {
        public void Configure(EntityTypeBuilder<PrescriptionMedicine> builder)
        {
            builder.ToTable("PrescriptionMedicines");

            builder.HasKey(pm => pm.Id);

            builder.Property(pm => pm.PrescriptionId)
                .IsRequired();

            builder.Property(pm => pm.MedicineId)
                .IsRequired();

            builder.Property(pm => pm.Dosage)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            builder.Property(pm => pm.DosageUnit)
                .IsRequired()
                .HasMaxLength(20)
                .HasColumnType("varchar(20)");

            builder.Property(pm => pm.Quantity)
                .IsRequired();

            builder.Property(pm => pm.Instructions)
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            builder.Property(pm => pm.CreatedAt)
                .IsRequired();

            builder.Property(pm => pm.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(pm => pm.DeletedAt);

            // Relationships
            builder.HasOne(pm => pm.Prescription)
                .WithMany(up => up.PrescriptionMedicines)
                .HasForeignKey(pm => pm.PrescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(pm => pm.Medicine)
                .WithMany(m => m.PrescriptionMedicines)
                .HasForeignKey(pm => pm.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

