using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Common;
using AGE.SignatureHub.Domain.Enums;
using AGE.SignatureHub.Domain.ValueObjects;

namespace AGE.SignatureHub.Domain.Entities
{
    public class Signer : BaseEntity
    {
        public Guid SignatureFlowId { get; private set; }
        public SignatureFlow SignatureFlow { get; private set; } = null!;
        public string Name { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string Document { get; private set; } = string.Empty; //CPF/CNPJ
        public SignerRole Role { get; private set; }
        public int SignOrder { get; private set; }
        public SignatureStatus Status { get; private set; }
        public SignatureType? SignatureType { get; private set; }
        public DateTime? SignedAt { get; private set; }
        public string RejectionReason { get; private set; } = string.Empty;
        public CertificateInfo CertificateInfo { get; private set; } = null!;
        public SignatureMetadata SignatureMetadata { get; private set; } = null!;
        public string SignatureImagePath { get; private set; } = string.Empty;

        /// <summary>
        /// Segredo de posse enviado no link de convite por e-mail. Concede acesso somente
        /// a este signatário/documento, sem exigir conta ou login interno (assinante externo).
        /// </summary>
        public string InvitationToken { get; private set; } = string.Empty;

        private Signer() { }

        public Signer(
                Guid signatureFlowId,
                string name,
                string email,
                string document,
                SignerRole role,
                int signOrder
            )
        {
            SignatureFlowId = signatureFlowId;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            Document = document ?? throw new ArgumentNullException(nameof(document));
            Role = role;
            SignOrder = signOrder > 0 ? signOrder : throw new ArgumentOutOfRangeException(nameof(signOrder), "Sign order must be greater than zero.");
            Status = SignatureStatus.Pending;
            InvitationToken = GenerateInvitationToken();
            CertificateInfo = new CertificateInfo(
                serialNumber: "PENDING",
                subjectName: "PENDING",
                issuerName: "PENDING",
                validFrom: DateTime.UtcNow,
                validTo: DateTime.UtcNow.AddYears(10),
                thumbprint: "PENDING"
            );
            SignatureMetadata = new SignatureMetadata(
                ipAddress: "0.0.0.0",
                userAgent: "PENDING",
                deviceInfo: "PENDING",
                location: "PENDING",
                documentHash: "PENDING"
            );
        }

        public void Sign(
                SignatureType signatureType, 
                SignatureMetadata signatureMetadata, 
                CertificateInfo certificateInfo,
                string? signatureImagePath 
            )
        {
            if (Status != SignatureStatus.Pending)
                throw new InvalidOperationException("Signer can only sign when status is pending.");

            SignatureType = signatureType;
            SignedAt = DateTime.UtcNow;
            CertificateInfo = certificateInfo;
            SignatureMetadata = signatureMetadata ?? throw new ArgumentNullException(nameof(signatureMetadata));
            SignatureImagePath = signatureImagePath ?? string.Empty;
            Status = SignatureStatus.Signed;
            SetUpdatedAt();
        }

        public void Reject(string reason)
        {
            if (Status != SignatureStatus.Pending)
                throw new InvalidOperationException("Signer can only reject when status is pending.");

            RejectionReason = reason ?? throw new ArgumentNullException(nameof(reason));
            Status = SignatureStatus.Rejected;
            SetUpdatedAt();
        }

        public void Cancel()
        {
            Status = SignatureStatus.Cancelled;
            SetUpdatedAt();
        }

        /// <summary>
        /// Compara em tempo constante para evitar vazamento de timing sobre o segredo do convite.
        /// </summary>
        public bool ValidateInvitationToken(string? token)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(InvitationToken))
            {
                return false;
            }

            var provided = System.Text.Encoding.UTF8.GetBytes(token);
            var expected = System.Text.Encoding.UTF8.GetBytes(InvitationToken);
            return CryptographicOperations.FixedTimeEquals(provided, expected);
        }

        private static string GenerateInvitationToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }
    }
}