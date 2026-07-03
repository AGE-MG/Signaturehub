using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.DTOs.Public
{
    public class PublicDocumentVerificationDto
    {
        public Guid DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string Title { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentHash { get; set; } = string.Empty;
        public DocumentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string VerificationUrl { get; set; } = string.Empty;
        public List<PublicSignedSignerDto> SignedSigners { get; set; } = new();
    }

    public class PublicSignedSignerDto
    {
        public Guid SignerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public SignatureType? SignatureType { get; set; }
        public DateTime? SignedAt { get; set; }
    }
}
