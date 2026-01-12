using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Common;

namespace AGE.SignatureHub.Domain.Entities
{
    public class DocumentVersion : BaseEntity
    {
        public Guid DocumentId { get; private set; }
        public int VersionNumber { get; private set; }
        public string StoragePath { get; private set; } = string.Empty;
        public string ContentHash { get; private set; } = string.Empty;
        public string changeDescription { get; private set; } = string.Empty;

        private DocumentVersion() { }

        public DocumentVersion(
                Guid documentId,
                int versionNumber,
                string storagePath,
                string contentHash,
                string changeDescription
            )
        {
            DocumentId = documentId;
            VersionNumber = versionNumber > 0 ? versionNumber : throw new ArgumentOutOfRangeException(nameof(versionNumber), "Version number must be greater than zero.");
            StoragePath = storagePath ?? throw new ArgumentNullException(nameof(storagePath));
            ContentHash = contentHash ?? throw new ArgumentNullException(nameof(contentHash));
            this.changeDescription = changeDescription;
        }
    }
}