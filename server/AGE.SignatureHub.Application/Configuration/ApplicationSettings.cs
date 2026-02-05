namespace AGE.SignatureHub.Application.Configuration
{
    public class ApplicationSettings
    {
        public const string SectionName = "Application";
        
        public string BaseUrl { get; set; } = string.Empty;
        public string SignatureUrlPath { get; set; } = "/sign";
        public EmailSettings Email { get; set; } = new();
        public WebhookSettings Webhook { get; set; } = new();
    }

    public class EmailSettings
    {
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = new int();
        public bool UseSsl { get; set; } = true;
    }

    public class WebhookSettings
    {
        public string DefaultEndpoint { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
