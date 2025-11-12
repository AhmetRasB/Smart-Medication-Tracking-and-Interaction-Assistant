using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.ToTable("Medicines");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(m => m.Description)
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            builder.Property(m => m.DosageForm)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(m => m.ActiveIngredient)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(m => m.Manufacturer)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(m => m.Barcode)
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(m => m.CreatedAt)
                .IsRequired();

            builder.Property(m => m.UpdatedAt);

            // Relationships
            builder.HasMany(m => m.MedicineSideEffects)
                .WithOne(mse => mse.Medicine)
                .HasForeignKey(mse => mse.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.PrescriptionMedicines)
                .WithOne(pm => pm.Medicine)
                .HasForeignKey(pm => pm.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

