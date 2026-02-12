using System.Collections.Generic;

namespace AGE.SignatureHub.Infrastructure.Configuration
{
    public class WebhookEndpoint
    {
        public string Name { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public List<string> Events { get; set; } = new List<string>();
    }
}
