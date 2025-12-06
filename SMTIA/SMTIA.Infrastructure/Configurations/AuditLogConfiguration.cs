using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(al => al.Id);

            builder.Property(al => al.UserId)
                .IsRequired();

            builder.Property(al => al.Action)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(al => al.EntityType)
                .IsRequired()
                .HasMaxLength(100)
                .HasColumnType("varchar(100)");

            builder.Property(al => al.EntityId);

            builder.Property(al => al.RequestPath)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            builder.Property(al => al.RequestMethod)
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(al => al.RequestBody)
                .HasColumnType("text");

            builder.Property(al => al.ResponseStatus)
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(al => al.IpAddress)
                .HasMaxLength(50)
                .HasColumnType("varchar(50)");

            builder.Property(al => al.UserAgent)
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");

            builder.Property(al => al.CreatedAt)
                .IsRequired();

            builder.Property(al => al.AdditionalData)
                .HasColumnType("text");

            builder.Property(al => al.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(al => al.DeletedAt);

            // Relationships
            builder.HasOne(al => al.User)
                .WithMany()
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes for better query performance
            builder.HasIndex(al => new { al.UserId, al.CreatedAt });
            builder.HasIndex(al => new { al.EntityType, al.EntityId });
            builder.HasIndex(al => al.Action);
        }
    }
}

