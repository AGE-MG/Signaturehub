using System.Collections.Generic;

namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class ActiveDirectoryDiagnosticDto
    {
        public string RequestedLogin { get; set; } = string.Empty;
        public string IdentityName { get; set; } = string.Empty;
        public string ResolvedAccountName { get; set; } = string.Empty;
        public string? EmailClaim { get; set; }
        public string? FullNameClaim { get; set; }
        public bool LookupSucceeded { get; set; }
        public string Message { get; set; } = string.Empty;
        public ActiveDirectoryLookupDto? Lookup { get; set; }
        public Dictionary<string, string?> Claims { get; set; } = new();
    }

    public class ActiveDirectoryLookupDto
    {
        public string AccountName { get; set; } = string.Empty;
        public string UserPrincipalName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? DistinguishedName { get; set; }
        public string? CanonicalName { get; set; }
        public List<string> OrganizationalUnits { get; set; } = new();
        public string? OrganizationalPath { get; set; }
    }
}
