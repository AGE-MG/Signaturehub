using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class DocumentConfiguration : IEntityTypeConfiguration<Document>
    {
        public void Configure(EntityTypeBuilder<Document> builder)
        {
            builder.ToTable("Documents");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.FileName)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(d => d.OriginalFileName)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(d => d.FileExtension)
                .IsRequired()
                .HasMaxLength(10);
            builder.Property(d => d.StoragePath)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(d => d.ContentHash)
                .IsRequired()
                .HasMaxLength(128);
            builder.Property(d => d.MimeType)
                .IsRequired()
                .HasMaxLength(100);
            builder.Property(d => d.Title)
                .IsRequired()
                .HasMaxLength(255);
            builder.Property(d => d.Description)
                .HasMaxLength(1000);
            builder.Property(d => d.Status)
                .IsRequired()
                .HasConversion<string>();
            builder.Property(d => d.CreatedByUserId)
                .IsRequired();
            builder.Property(d => d.CreatedAt)
                .IsRequired();
            builder.Property(d => d.UpdatedAt)
                .IsRequired();
            builder.Property(d => d.ExpiresAt)
                .IsRequired(false);
            builder.Property(d => d.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasMany(d => d.SignatureFlows)
                .WithOne(sf => sf.Document)
                .HasForeignKey(sf => sf.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.HasMany(d => d.Versions)
                .WithOne()
                .HasForeignKey(al => al.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasIndex(d => d.CreatedByUserId);
            builder.HasIndex(d => d.Status);
            builder.HasIndex(d => d.CreatedAt);
            builder.HasIndex(d => d.ExpiresAt);
            builder.HasIndex(d => d.ContentHash);
        }
    }
}