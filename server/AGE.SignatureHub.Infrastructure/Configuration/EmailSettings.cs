using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AGE.SignatureHub.Infrastructure.Configuration
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }

        public EmailSettings(string smtpServer, int smtpPort, string senderName, string senderEmail, string username, string password, bool useSsl)
        {
            SmtpServer = smtpServer;
            SmtpPort = smtpPort;
            SenderName = senderName;
            SenderEmail = senderEmail;
            Username = username;
            Password = password;
            UseSsl = useSsl;
        }
    }
}