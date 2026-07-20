using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations;

public sealed class ExternalServiceConnectionConfiguration : IEntityTypeConfiguration<ExternalServiceConnection>
{
    public void Configure(EntityTypeBuilder<ExternalServiceConnection> builder)
    {
        builder.ToTable("ExternalServiceConnections");
        builder.HasKey(connection => connection.Id);
        builder.Property(connection => connection.Name).HasMaxLength(100).IsRequired();
        builder.Property(connection => connection.Url).HasMaxLength(2048).IsRequired();
        builder.Property(connection => connection.Secret).HasMaxLength(256).IsRequired();
        builder.Property(connection => connection.EventsCsv).HasMaxLength(1000).IsRequired();
        builder.HasIndex(connection => connection.UserId);
        builder.HasIndex(connection => new { connection.UserId, connection.Name }).IsUnique();
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(connection => connection.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
