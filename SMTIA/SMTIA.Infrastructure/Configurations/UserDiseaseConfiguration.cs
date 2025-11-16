using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class UserDiseaseConfiguration : IEntityTypeConfiguration<UserDisease>
    {
        public void Configure(EntityTypeBuilder<UserDisease> builder)
        {
            builder.ToTable("UserDiseases");

            builder.HasKey(ud => ud.Id);

            builder.Property(ud => ud.UserId)
                .IsRequired();

            builder.Property(ud => ud.DiseaseName)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(ud => ud.Description)
                .HasMaxLength(1000)
                .HasColumnType("varchar(1000)");

            builder.Property(ud => ud.DiagnosisDate);

            builder.Property(ud => ud.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(ud => ud.CreatedAt)
                .IsRequired();

            builder.Property(ud => ud.UpdatedAt);

            builder.Property(ud => ud.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ud => ud.DeletedAt);

            // Relationships
            builder.HasOne(ud => ud.User)
                .WithMany(u => u.UserDiseases)
                .HasForeignKey(ud => ud.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

