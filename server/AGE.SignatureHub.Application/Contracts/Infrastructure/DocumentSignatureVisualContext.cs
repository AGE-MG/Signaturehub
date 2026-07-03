using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.Contracts.Infrastructure
{
    public class DocumentSignatureVisualContext
    {
        public Guid DocumentId { get; set; }
        public string DocumentTitle { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public string VerificationUrl { get; set; } = string.Empty;
        public SignatureType CurrentSignatureType { get; set; }
        public List<SignedSignerVisualInfo> SignedSigners { get; set; } = new();
    }

    public class SignedSignerVisualInfo
    {
        public Guid SignerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public SignatureType SignatureType { get; set; }
        public DateTime SignedAt { get; set; }
    }
}
