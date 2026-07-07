namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class WindowsSsoLoginRequest
    {
        public string IdentityName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? FullName { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? RegistrationNumber { get; set; }
        public bool RememberMe { get; set; } = true;
    }
}
