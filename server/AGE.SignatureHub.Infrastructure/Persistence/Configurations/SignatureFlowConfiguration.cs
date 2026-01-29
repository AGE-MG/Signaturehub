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
        }
    }
}