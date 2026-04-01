namespace AGE.SignatureHub.Application.Configuration
{
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; } = "AGE SignatureHub";
        public string ApplicationUrl { get; set; } = "https://signaturehub.age.mg.gov.br";
        public int MaxDocumentSizeInMB { get; set; } = 50;
        public int DocumentExpirationInDays { get; set; } = 30;
        public bool EnableEmailNotifications { get; set; } = true;
        public bool EnableWebhooks { get; set; } = false;
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
        public List<WebhookEndpoint> Endpoints { get; set; } = new List<WebhookEndpoint>();
        public string DefaultEndpoint { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }

    public class WebhookEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public List<string> Events { get; set; } = new List<string>();
    }
}
