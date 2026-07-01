namespace AGE.SignatureHub.Application.DTOs.Auth
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? RegistrationNumber { get; set; }
    }
}
