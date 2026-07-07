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
        public bool EnableWindowsSso { get; set; } = false;
        public bool AllowLocalFallback { get; set; } = true;
        public string DefaultRole { get; set; } = "User";
        public bool LookupUseDefaultCredentials { get; set; } = true;
        public string LookupBindUser { get; set; } = string.Empty;
        public string LookupBindPassword { get; set; } = string.Empty;
        public string[] DisplayNameAttributes { get; set; } = ["displayName", "cn", "name", "givenName"];
        public string[] DepartmentAttributes { get; set; } = ["department"];
        public string[] PositionAttributes { get; set; } = ["title"];
        public string[] RegistrationNumberAttributes { get; set; } = ["employeeID", "employeeNumber", "extensionAttribute1"];
    }
}
