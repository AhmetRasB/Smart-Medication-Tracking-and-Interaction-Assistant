using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class UserAllergyConfiguration : IEntityTypeConfiguration<UserAllergy>
    {
        public void Configure(EntityTypeBuilder<UserAllergy> builder)
        {
            builder.ToTable("UserAllergies");

            builder.HasKey(ua => ua.Id);

            builder.Property(ua => ua.UserId)
                .IsRequired();

            builder.Property(ua => ua.AllergyName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(ua => ua.Description)
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            builder.Property(ua => ua.Severity)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(ua => ua.CreatedAt)
                .IsRequired();

            builder.Property(ua => ua.UpdatedAt);

            builder.Property(ua => ua.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ua => ua.DeletedAt);

            // Relationships
            builder.HasOne(ua => ua.User)
                .WithMany(u => u.UserAllergies)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

