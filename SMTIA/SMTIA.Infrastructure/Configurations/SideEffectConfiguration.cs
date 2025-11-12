using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class SideEffectConfiguration : IEntityTypeConfiguration<SideEffect>
    {
        public void Configure(EntityTypeBuilder<SideEffect> builder)
        {
            builder.ToTable("SideEffects");

            builder.HasKey(se => se.Id);

            builder.Property(se => se.Name)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(se => se.Description)
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            builder.Property(se => se.Severity)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(se => se.CreatedAt)
                .IsRequired();

            // Relationships
            builder.HasMany(se => se.MedicineSideEffects)
                .WithOne(mse => mse.SideEffect)
                .HasForeignKey(mse => mse.SideEffectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

