namespace AGE.SignatureHub.Infrastructure.Configuration
{
    public class ActiveDirectorySettings
    {
        public const string SectionName = "ActiveDirectory";

        public bool Enabled { get; set; }
        public string Server { get; set; } = string.Empty;
        public int Port { get; set; } = 389;
        public string Domain { get; set; } = string.Empty;
        public string UserPrincipalNameSuffix { get; set; } = string.Empty;
        public string SearchBase { get; set; } = string.Empty;
        public string EmailDomain { get; set; } = string.Empty;
        public bool UseSsl { get; set; }
        public bool AllowLocalFallback { get; set; } = true;
        public string DefaultRole { get; set; } = "User";
    }
}
