using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.ToTable("Notifications");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.UserId)
                .IsRequired();

            builder.Property(n => n.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(n => n.RelatedDocumentId)
                .IsRequired(false);

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.Property(n => n.UpdatedAt)
                .IsRequired();

            builder.HasIndex(n => n.UserId);
            builder.HasIndex(n => n.IsRead);
            builder.HasIndex(n => new { n.UserId, n.IsRead });
            builder.HasIndex(n => n.CreatedAt);
        }
    }
}