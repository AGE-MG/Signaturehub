using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public void Sign(
                SignatureType signatureType, 
                SignatureMetadata signatureMetadata, 
                CertificateInfo certificateInfo,
                string signatureImagePath 
            )
        {
            if (Status != SignatureStatus.Pending)
                throw new InvalidOperationException("Signer can only sign when status is pending.");

            SignatureType = signatureType;
            SignedAt = DateTime.UtcNow;
            CertificateInfo = certificateInfo;
            SignatureMetadata = signatureMetadata ?? throw new ArgumentNullException(nameof(signatureMetadata));
            SignatureImagePath = signatureImagePath;
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
    }
}