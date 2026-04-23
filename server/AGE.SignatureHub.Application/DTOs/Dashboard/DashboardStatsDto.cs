namespace AGE.SignatureHub.Application.DTOs.Dashboard
{
    public class DashboardStatsDto
    {
        public int TotalDocuments { get; set; }
        public int DraftDocuments { get; set; }
        public int PendingDocuments { get; set; }
        public int CompletedDocuments { get; set; }
        public int RejectedDocuments { get; set; }
        public int ExpiredDocuments { get; set; }
        public int UnreadNotifications { get; set; }
    }
}
