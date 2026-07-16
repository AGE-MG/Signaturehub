using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Infrastructure.Configuration;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly ActiveDirectorySettings _activeDirectorySettings;
        private readonly AGE.SignatureHub.Application.Configuration.EmailSettings _emailSettings;
        private readonly AGE.SignatureHub.Application.Configuration.WebhookSettings _webhookSettings;

        public AuthController(
            ILogger<AuthController> logger,
            IAuthService authService,
            IOptions<ActiveDirectorySettings> activeDirectorySettings,
            IOptions<AGE.SignatureHub.Application.Configuration.EmailSettings> emailSettings,
            IOptions<AGE.SignatureHub.Application.Configuration.WebhookSettings> webhookSettings)
        {
            _logger = logger;
            _authService = authService;
            _activeDirectorySettings = activeDirectorySettings.Value;
            _emailSettings = emailSettings.Value;
            _webhookSettings = webhookSettings.Value;
        }

        [HttpGet("notification-capabilities")]
        [Authorize]
        public IActionResult GetNotificationCapabilities()
        {
            var emailConfigured = IsRealSetting(_emailSettings.SmtpServer) &&
                                  IsRealSetting(_emailSettings.SenderEmail);

            return Ok(new
            {
                success = true,
                data = new
                {
                    emailConfigured,
                    externalServices = _webhookSettings.Endpoints
                        .Where(endpoint => !string.IsNullOrWhiteSpace(endpoint.Name) && !string.IsNullOrWhiteSpace(endpoint.Url))
                        .Select(endpoint => new { endpoint.Name, eventCount = endpoint.Events.Count })
                        .ToArray()
                }
            });
        }

        private static bool IsRealSetting(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            var normalized = value.Trim().ToLowerInvariant();
            return !normalized.Contains("example.com") &&
                   !normalized.StartsWith("your_") &&
                   !normalized.StartsWith("change_me");
        }

        /// <summary>
        /// Executes the user login process
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Executes Windows/Active Directory SSO using the current machine user.
        /// </summary>
        [HttpGet("windows-sso")]
        [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> WindowsSso()
        {
            if (!_activeDirectorySettings.EnableWindowsSso)
            {
                return BadRequest(new { success = false, message = "Windows SSO is disabled." });
            }

            var request = new WindowsSsoLoginRequest
            {
                IdentityName = User.Identity?.Name ?? string.Empty,
                Email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Upn),
                FullName = User.FindFirstValue(ClaimTypes.Name),
                RememberMe = true
            };

            var result = await _authService.WindowsSsoLoginAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// Returns a diagnostic JSON payload for the current Active Directory user lookup.
        /// </summary>
        [HttpGet("windows-sso/diagnostic")]
        [Authorize(AuthenticationSchemes = NegotiateDefaults.AuthenticationScheme)]
        [ProducesResponseType(typeof(ActiveDirectoryDiagnosticDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> WindowsSsoDiagnostic([FromQuery] string? login = null)
        {
            if (!_activeDirectorySettings.EnableWindowsSso)
            {
                return BadRequest(new { success = false, message = "Windows SSO is disabled." });
            }

            var claims = User.Claims
                .GroupBy(claim => claim.Type)
                .ToDictionary(group => group.Key, group => group.FirstOrDefault()?.Value);

            var identityName = User.Identity?.Name ?? string.Empty;
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Upn);
            var fullName = User.FindFirstValue(ClaimTypes.Name);

            var result = await _authService.GetActiveDirectoryDiagnosticAsync(
                identityName,
                login,
                email,
                fullName,
                claims);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Renew the access token using a refresh token
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return result.Success ? Ok(result) : Unauthorized(result);
        }

        /// <summary>
        /// execute the user logout process
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Logout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.LogoutAsync(userId);
            return HandleResponse(result);
        }

        /// <summary>
        /// Register a new user account
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return HandleCreatedResponse(result, nameof(Register), new { id = result.Data?.Id });
        }

        /// <summary>
        /// Get the current authenticated user's information
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.GetCurrentUserAsync(userId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        /// <summary>
        /// Change the current authenticated user's password
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            return HandleResponse(result);
        }
    }
}
