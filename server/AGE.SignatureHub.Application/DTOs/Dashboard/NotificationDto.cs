using AGE.SignatureHub.Domain.Enums;

namespace AGE.SignatureHub.Application.DTOs.Dashboard
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public Guid? RelatedDocumentId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
