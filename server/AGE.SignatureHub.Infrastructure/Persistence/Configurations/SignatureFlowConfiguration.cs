using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class SignatureFlowConfiguration : IEntityTypeConfiguration<SignatureFlow>
    {
    public void Configure(EntityTypeBuilder<SignatureFlow> builder)
        {
            builder.ToTable("SignatureFlows");

            builder.HasKey(sf => sf.Id);

            builder.Property(sf => sf.DocumentId)
                .IsRequired();

            builder.Property(sf => sf.FlowName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sf => sf.FlowType)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(sf => sf.CurrentStep)
                .IsRequired();

            builder.Property(sf => sf.TotalSteps)
                .IsRequired();

            builder.Property(sf => sf.IsCompleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(sf => sf.CompletedAt)
                .IsRequired();

            builder.Property(sf => sf.CreatedAt)
                .IsRequired();

            builder.Property(sf => sf.UpdatedAt);

            builder.Property(sf => sf.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasMany(sf => sf.Signers)
                .WithOne(s => s.SignatureFlow)
                .HasForeignKey(s => s.SignatureFlowId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(sf => sf.DocumentId);
            builder.HasIndex(sf => sf.IsCompleted);
        }
    }
}