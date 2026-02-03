using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.DocumentId);

            builder.Property(a => a.SignerId);

            builder.Property(a => a.UserId);

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.Details)
                .HasMaxLength(2000);

            builder.Property(a => a.IpAddress)
                .HasMaxLength(50);

            builder.Property(a => a.UserAgent)
                .HasMaxLength(500);

            builder.Property(a => a.Timestamp)
                .IsRequired();

            builder.HasIndex(a => a.DocumentId);
            builder.HasIndex(a => a.SignerId);
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.Timestamp);
            builder.HasIndex(a => a.Action);
        }
    }
}