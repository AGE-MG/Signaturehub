using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.Property(u => u.NetworkUserName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(u => u.FullName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(u => u.Department)
                .HasMaxLength(255);

            builder.Property(u => u.Position)
                .HasMaxLength(255);

            builder.Property(u => u.RegistrationNumber)
                .HasMaxLength(100);

            builder.HasIndex(u => u.NetworkUserName);
            builder.HasIndex(u => u.Department);
        }
    }
}
