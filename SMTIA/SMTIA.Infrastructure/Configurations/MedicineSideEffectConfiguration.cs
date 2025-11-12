using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class MedicineSideEffectConfiguration : IEntityTypeConfiguration<MedicineSideEffect>
    {
        public void Configure(EntityTypeBuilder<MedicineSideEffect> builder)
        {
            builder.ToTable("MedicineSideEffects");

            builder.HasKey(mse => mse.Id);

            builder.Property(mse => mse.MedicineId)
                .IsRequired();

            builder.Property(mse => mse.SideEffectId)
                .IsRequired();

            builder.Property(mse => mse.Frequency)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            // Relationships
            builder.HasOne(mse => mse.Medicine)
                .WithMany(m => m.MedicineSideEffects)
                .HasForeignKey(mse => mse.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(mse => mse.SideEffect)
                .WithMany(se => se.MedicineSideEffects)
                .HasForeignKey(mse => mse.SideEffectId)
                .OnDelete(DeleteBehavior.Cascade);

            // Unique constraint: A medicine can have a side effect only once
            builder.HasIndex(mse => new { mse.MedicineId, mse.SideEffectId })
                .IsUnique();
        }
    }
}

