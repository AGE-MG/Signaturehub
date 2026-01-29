using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
    {
        public void Configure(EntityTypeBuilder<DocumentVersion> builder)
        {
            builder.ToTable("DocumentVersions");

            builder.HasKey(dv => dv.Id);

            builder.Property(dv => dv.DocumentId)
                .IsRequired();
            builder.Property(dv => dv.VersionNumber)
                .IsRequired();
            builder.Property(dv => dv.StoragePath)
                .IsRequired()
                .HasMaxLength(500);
            builder.Property(dv => dv.ContentHash)
                .IsRequired()
                .HasMaxLength(128);
            builder.Property(dv => dv.changeDescription)
                .HasMaxLength(500);
            builder.Property(dv => dv.CreatedAt)
                .IsRequired();

            builder.HasIndex(dv => dv.DocumentId);
            builder.HasIndex(dv => new { dv.DocumentId, dv.VersionNumber })
                .IsUnique();
        }
    }
}