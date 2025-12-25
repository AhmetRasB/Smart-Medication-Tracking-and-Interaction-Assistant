using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SMTIA.Domain.Entities;

namespace SMTIA.Infrastructure.Configurations
{
    internal sealed class InteractionAnalysisConfiguration : IEntityTypeConfiguration<InteractionAnalysis>
    {
        public void Configure(EntityTypeBuilder<InteractionAnalysis> builder)
        {
            builder.ToTable("InteractionAnalyses");

            builder.HasKey(ia => ia.Id);

            builder.Property(ia => ia.UserId)
                .IsRequired();

            builder.Property(ia => ia.NewMedicineId);

            builder.Property(ia => ia.NewMedicineName)
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            builder.Property(ia => ia.ExistingMedicinesJson)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(ia => ia.AllergiesJson)
                .HasColumnType("text");

            builder.Property(ia => ia.RiskLevel)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(ia => ia.Summary)
                .IsRequired()
                .HasMaxLength(2000)
                .HasColumnType("varchar(2000)");

            builder.Property(ia => ia.DetailedAnalysis)
                .HasColumnType("text");

            builder.Property(ia => ia.Recommendations)
                .HasColumnType("text");

            builder.Property(ia => ia.RawAiResponse)
                .HasColumnType("text");

            builder.Property(ia => ia.CreatedAt)
                .IsRequired();

            builder.Property(ia => ia.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(ia => ia.DeletedAt);

            // Relationships
            builder.HasOne(ia => ia.User)
                .WithMany()
                .HasForeignKey(ia => ia.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ia => ia.NewMedicine)
                .WithMany()
                .HasForeignKey(ia => ia.NewMedicineId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes for better query performance
            builder.HasIndex(ia => new { ia.UserId, ia.CreatedAt });
            builder.HasIndex(ia => ia.RiskLevel);
        }
    }
}

