using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Infrastructure.Configuration
{
    public class WebhookSettings
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string Secret { get; set; }
        public List<string> Events { get; set; } = new List<string>();

        public WebhookSettings(string name, string url, string secret, List<string> events)
        {
            Name = name;
            Url = url;
            Secret = secret;
            Events = events;
        }
    }
}