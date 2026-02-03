using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AGE.SignatureHub.Infrastructure.Persistence.Configurations
{
    public class SignerConfiguration : IEntityTypeConfiguration<Signer>
    {
        public void Configure(EntityTypeBuilder<Signer> builder)
        {
            builder.ToTable("Signers");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.SignatureFlowId)
                .IsRequired();

            builder.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(s => s.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(s => s.Document)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(s => s.Role)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(s => s.SignOrder)
                .IsRequired();

            builder.Property(s => s.Status)
                .IsRequired()
                .HasConversion<string>();

            builder.Property(s => s.SignatureType)
                .HasConversion<string>();

            builder.Property(s => s.SignedAt);

            builder.Property(s => s.RejectionReason)
                .HasMaxLength(500);

            builder.Property(s => s.SignatureImagePath)
                .HasMaxLength(255);
            
            builder.Property(s => s.CreatedAt)
                .IsRequired();

            builder.Property(s => s.UpdatedAt);

            builder.Property(s => s.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);

            builder.OwnsOne(s => s.CertificateInfo, cert =>
            {
                cert.Property(c => c.SubjectName)
                    .HasMaxLength(500)
                    .HasColumnName("CertificateSubjectName");

                cert.Property(c => c.SerialNumber)
                    .HasMaxLength(100)
                    .HasColumnName("CertificateSerialNumber");
                
                cert.Property(c => c.IssuerName)
                    .HasMaxLength(500)
                    .HasColumnName("CertificateIssuerName");

                cert.Property(c => c.ValidFrom)
                    .HasColumnName("CertificateValidFrom");

                cert.Property(c => c.ValidTo)
                    .HasColumnName("CertificateValidTo");

                cert.Property(c => c.Thumbprint)
                    .HasMaxLength(100)
                    .HasColumnName("CertificateThumbprint");

                cert.Property(c => c.isValid)
                    .HasColumnName("CertificateIsValid");
            });

            builder.OwnsOne(s => s.SignatureMetadata, meta =>
            {
                meta.Property(m => m.IpAddress)
                    .HasMaxLength(45)
                    .HasColumnName("SignatureIPAddress");

                meta.Property(m => m.UserAgent)
                    .HasMaxLength(500)
                    .HasColumnName("SignatureUserAgent");

                meta.Property(m => m.DeviceInfo)
                    .HasMaxLength(100)
                    .HasColumnName("SignatureDeviceInfo");

                meta.Property(m => m.Location)
                    .HasMaxLength(255)
                    .HasColumnName("SignatureLocation");

                meta.Property(m => m.DocumentHash)
                    .HasMaxLength(128)
                    .HasColumnName("SignatureDocumentHash");
            });

            builder.HasIndex(s => s.SignatureFlowId);
            builder.HasIndex(s => s.Email);
            builder.HasIndex(s => s.Status);
            builder.HasIndex(s => new { s.SignatureFlowId, s.SignOrder });
        }
    }
}