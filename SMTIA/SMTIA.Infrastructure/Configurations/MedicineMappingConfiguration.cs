using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class MedicineMappingConfiguration : IEntityTypeConfiguration<MedicineMapping>
    {
        public void Configure(EntityTypeBuilder<MedicineMapping> builder)
        {
            builder.ToTable("MedicineMappings");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.QueryTerm)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(x => x.BrandNameTr)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(x => x.ActiveIngredientTr)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(x => x.ActiveIngredientEn)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Source)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(x => x.Confidence)
                .IsRequired()
                .HasColumnType("decimal(5,4)");

            builder.Property(x => x.ConfirmedByUserId);

            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt);

            builder.Property(x => x.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DeletedAt);

            builder.HasOne(x => x.ConfirmedByUser)
                .WithMany()
                .HasForeignKey(x => x.ConfirmedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(x => x.QueryTerm);
            builder.HasIndex(x => new { x.Status, x.Source });
        }
    }
}


