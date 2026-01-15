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
        public string FromEmail { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
    }

    public class WebhookSettings
    {
        public string DefaultEndpoint { get; set; } = string.Empty;
        public int TimeoutSeconds { get; set; } = 30;
    }
}
