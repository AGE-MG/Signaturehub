using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Common;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Domain.Entities
{
    public class Document : BaseEntity
    {
        public string FileName { get; private set; } = string.Empty;
        public string OriginalFileName { get; private set; } = string.Empty;
        public string FileExtension { get; private set; } = string.Empty;
        public long FileSizeInBytes { get; private set; }
        public string StoragePath { get; private set; } = string.Empty;
        public string ContentHash { get; private set; } = string.Empty;
        public string MimeType { get; private set; } = string.Empty;
        public DocumentStatus Status { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;
        public DateTime? ExpiresAt { get; private set; }
        public Guid CreatedByUserId { get; private set; }

        private readonly List<SignatureFlow> _signatureFlows = new();
        public IReadOnlyCollection<SignatureFlow> SignatureFlows => _signatureFlows.AsReadOnly();

        private readonly List<DocumentVersion> _versions = new();
        public IReadOnlyCollection<DocumentVersion> Versions => _versions.AsReadOnly();

        private Document() { }

        public Document(
                string fileName,
                string originalFileName,
                string fileExtension,
                long fileSizeInBytes,
                string storagePath,
                string contentHash,
                string mimeType,
                string title,
                string description,
                Guid createdByUserId,
                DateTime? expiresAt = null
            )
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            OriginalFileName = originalFileName ?? throw new ArgumentNullException(nameof(originalFileName));
            FileExtension = fileExtension ?? throw new ArgumentNullException(nameof(fileExtension));
            FileSizeInBytes = fileSizeInBytes > 0 ? fileSizeInBytes : throw new ArgumentOutOfRangeException(nameof(fileSizeInBytes), "File size must be greater than zero.");
            StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
            ContentHash = contentHash ?? throw new ArgumentNullException(nameof(contentHash));
            MimeType = mimeType ?? throw new ArgumentNullException(nameof(mimeType));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Status = DocumentStatus.Draft;
            Description = description;
            ExpiresAt = expiresAt;
            CreatedByUserId = createdByUserId;

            AddVersion(1, storagePath, contentHash, "Initial version");
        }

        public void UpdateStatus(DocumentStatus newStatus)
        {
            Status = newStatus;
            SetUpdatedAt();
        }

        public void AddSignatureFlow(SignatureFlow flow)
        {
            _signatureFlows.Add(flow);
            if (Status == DocumentStatus.Draft)
            {
                UpdateStatus(DocumentStatus.PendingSignatures);
            }
            SetUpdatedAt();
        }

        public void AddVersion(int versionNumber, string storagePath, string contentHash, string changeDescription)
        {
            var version = new DocumentVersion(Id, versionNumber, storagePath, contentHash, changeDescription);
            _versions.Add(version);
            SetUpdatedAt();
        }

        public bool IsExpired()
        {
            return ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;
        }

        public void CheckAndUpdateExpiration()
        {
            if (IsExpired() && Status != DocumentStatus.Completed && Status != DocumentStatus.Cancelled)
            {
                UpdateStatus(DocumentStatus.Expired);
            }
        }
    }
}