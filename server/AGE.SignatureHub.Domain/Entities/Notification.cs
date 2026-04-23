using AGE.SignatureHub.Domain.Common;
using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; private set; }
        public string Title { get; private set; } = string.Empty;
        public string Message { get; private set; } = string.Empty;
        public NotificationType Type { get; private set; }
        public bool IsRead { get; private set; }
        public Guid? RelatedDocumentId { get; private set; }

        private Notification() { }

        public Notification(Guid userId, string title, string message, NotificationType type, Guid? relatedDocumentId = null)
        {
            UserId = userId;
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Type = type;
            IsRead = false;
            RelatedDocumentId = relatedDocumentId;
        }

        public void MarkAsRead()
        {
            IsRead = true;
            SetUpdatedAt();
        }
    }
}
