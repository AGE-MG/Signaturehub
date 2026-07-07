using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Infrastructure.Configuration;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace AGE.SignatureHub.Infrastructure.Services.Identity
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ActiveDirectoryAuthenticationService _activeDirectoryAuthenticationService;
        private readonly ActiveDirectorySettings _activeDirectorySettings;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IMapper mapper,
            ActiveDirectoryAuthenticationService activeDirectoryAuthenticationService,
            IOptions<ActiveDirectorySettings> activeDirectorySettings,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _activeDirectoryAuthenticationService = activeDirectoryAuthenticationService;
            _activeDirectorySettings = activeDirectorySettings.Value;
            _logger = logger;
        }
        public async Task<BaseResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var response = new BaseResponse<bool>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    throw new NotFoundException(nameof(ApplicationUser), userId);
                }

                var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

                if (!result.Succeeded)
                {
                    response.Success = false;
                    response.Message = "Password change failed.";
                    response.Errors = result.Errors.Select(e => e.Description).ToList();
                    return response;
                }

                response.Success = true;
                response.Message = "Password changed successfully.";
                response.Data = true;
                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user {UserId}", userId);
                response.Success = false;
                response.Message = "An error occurred while changing the password.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        public async Task<BaseResponse<UserDto>> GetCurrentUserAsync(string userId)
        {
            var response = new BaseResponse<UserDto>();

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    throw new NotFoundException(nameof(ApplicationUser), userId);
                }

                var roles = await _userManager.GetRolesAsync(user);
                var userDto = _mapper.Map<UserDto>(user);

                userDto.Roles = roles.ToList();
                response.Success = true;
                response.Data = userDto;
                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving current user with ID {UserId}", userId);
                response.Success = false;
                response.Message = "An error occurred while retrieving the current user.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var response = new BaseResponse<LoginResponse>();

            try
            {
                if (request.LoginMode == LoginMode.ActiveDirectory)
                {
                    if (!_activeDirectoryAuthenticationService.IsEnabled)
                    {
                        response.Success = false;
                        response.Message = "Active Directory login is disabled.";
                        return response;
                    }

                    var adResult = await _activeDirectoryAuthenticationService.AuthenticateAsync(request.Email, request.Password);
                    if (adResult != null)
                    {
                        return await SignInWithActiveDirectoryAsync(adResult, request.RememberMe);
                    }

                    response.Success = false;
                    response.Message = "Invalid network username or password.";
                    return response;
                }

                var normalizedLogin = request.Email.Trim();
                var user = await _userManager.FindByEmailAsync(normalizedLogin);

                if (user == null && !normalizedLogin.Contains('@'))
                {
                    user = await _userManager.Users.FirstOrDefaultAsync(u => u.NetworkUserName == normalizedLogin, CancellationToken.None);
                }

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Invalid internal username or password.";
                    return response;
                }

                if (!user.IsActive)
                {
                    response.Success = false;
                    response.Message = "User account is inactive.";
                    return response;
                }

                var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, request.RememberMe, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        _logger.LogWarning("User {Login} account locked out.", request.Email);
                        response.Success = false;
                        response.Message = "Account locked due to multiple failed login attempts. Please try again later.";
                        return response;
                    }

                    response.Success = false;
                    response.Message = "Invalid internal username or password.";
                    return response;
                }

                // Update last login
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Update refresh token
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = roles.ToList();

                response.Success = true;
                response.Message = "Login successful.";
                response.Data = new LoginResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    TokenExpiration = DateTime.UtcNow.AddHours(1),
                    User = userDto
                };

                _logger.LogInformation("User {Login} logged in successfully via internal login", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", request.Email);
                response.Success = false;
                response.Message = "An error occurred during login.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        public async Task<BaseResponse<LoginResponse>> WindowsSsoLoginAsync(WindowsSsoLoginRequest request)
        {
            var response = new BaseResponse<LoginResponse>();

            try
            {
                if (!_activeDirectorySettings.EnableWindowsSso)
                {
                    response.Success = false;
                    response.Message = "Windows SSO is disabled.";
                    return response;
                }

                if (string.IsNullOrWhiteSpace(request.IdentityName))
                {
                    response.Success = false;
                    response.Message = "Windows identity was not provided.";
                    return response;
                }

                var accountName = ExtractAccountName(request.IdentityName);
                if (string.IsNullOrWhiteSpace(accountName))
                {
                    response.Success = false;
                    response.Message = "Could not resolve the Windows account name.";
                    return response;
                }

                var adLookup = await _activeDirectoryAuthenticationService.LookupUserAsync(accountName, request.Email, CancellationToken.None);

                var adResult = adLookup ?? new ActiveDirectoryAuthenticationResult
                {
                    AccountName = accountName,
                    UserPrincipalName = BuildUserPrincipalName(accountName),
                    Email = BuildEmail(accountName, request.Email),
                    DisplayName = string.IsNullOrWhiteSpace(request.FullName) ? accountName : request.FullName.Trim(),
                    Department = NormalizeOptional(request.Department),
                    Position = NormalizeOptional(request.Position),
                    RegistrationNumber = NormalizeOptional(request.RegistrationNumber),
                };

                if (adLookup == null && !string.IsNullOrWhiteSpace(request.FullName))
                {
                    adResult = new ActiveDirectoryAuthenticationResult
                    {
                        AccountName = adResult.AccountName,
                        UserPrincipalName = adResult.UserPrincipalName,
                        Email = adResult.Email,
                        DisplayName = request.FullName.Trim(),
                        Department = adResult.Department,
                        Position = adResult.Position,
                        RegistrationNumber = adResult.RegistrationNumber,
                    };
                }

                return await SignInWithActiveDirectoryAsync(adResult, request.RememberMe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Windows SSO login for identity {IdentityName}", request.IdentityName);
                response.Success = false;
                response.Message = "An error occurred during Windows SSO login.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        private async Task<BaseResponse<LoginResponse>> SignInWithActiveDirectoryAsync(ActiveDirectoryAuthenticationResult adResult, bool rememberMe)
        {
            var response = new BaseResponse<LoginResponse>();

            var email = adResult.Email.Trim().ToLowerInvariant();
            var networkUserName = adResult.AccountName.Trim();
            var user = await _userManager.FindByEmailAsync(email);
            user ??= await _userManager.Users.FirstOrDefaultAsync(u => u.NetworkUserName == networkUserName);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    NetworkUserName = networkUserName,
                    UserName = email,
                    Email = email,
                    FullName = string.IsNullOrWhiteSpace(adResult.DisplayName) ? networkUserName : adResult.DisplayName.Trim(),
                    Department = string.IsNullOrWhiteSpace(adResult.Department) ? null : adResult.Department.Trim(),
                    Position = string.IsNullOrWhiteSpace(adResult.Position) ? null : adResult.Position.Trim(),
                    RegistrationNumber = string.IsNullOrWhiteSpace(adResult.RegistrationNumber) ? null : adResult.RegistrationNumber.Trim(),
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                if (!createResult.Succeeded)
                {
                    response.Success = false;
                    response.Message = "Could not provision the Active Directory user locally.";
                    response.Errors = createResult.Errors.Select(e => e.Description).ToList();
                    return response;
                }

                if (await _roleManager.RoleExistsAsync("User"))
                {
                    await _userManager.AddToRoleAsync(user, "User");
                }
                else
                {
                    _logger.LogWarning("Default role 'User' was not found while provisioning Active Directory user {Email}", email);
                }
            }
            else
            {
                user.NetworkUserName = networkUserName;
                user.UserName = email;
                user.Email = email;
                user.FullName = string.IsNullOrWhiteSpace(adResult.DisplayName) ? user.FullName : adResult.DisplayName.Trim();
                user.Department = string.IsNullOrWhiteSpace(adResult.Department) ? null : adResult.Department.Trim();
                user.Position = string.IsNullOrWhiteSpace(adResult.Position) ? null : adResult.Position.Trim();
                user.RegistrationNumber = string.IsNullOrWhiteSpace(adResult.RegistrationNumber) ? null : adResult.RegistrationNumber.Trim();
                user.IsActive = true;
            }

            if (!user.IsActive)
            {
                response.Success = false;
                response.Message = "User account is inactive.";
                return response;
            }

            user.LastLoginAt = DateTime.UtcNow;
            user.RefreshToken = _tokenService.GenerateRefreshToken();
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(rememberMe ? 14 : 7);
            await _userManager.UpdateAsync(user);

            var roles = await _userManager.GetRolesAsync(user);
            var accessToken = _tokenService.GenerateAccessToken(user, roles);
            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = roles.ToList();

            response.Success = true;
            response.Message = "Login successful.";
            response.Data = new LoginResponse
            {
                Token = accessToken,
                RefreshToken = user.RefreshToken,
                TokenExpiration = DateTime.UtcNow.AddHours(1),
                User = userDto
            };

            _logger.LogInformation("User {Email} logged in successfully via Active Directory", email);
            return response;
        }

        private string ExtractAccountName(string identityName)
        {
            var normalized = identityName.Trim();

            if (normalized.Contains('\\'))
            {
                return normalized[(normalized.LastIndexOf('\\') + 1)..];
            }

            if (normalized.Contains('@'))
            {
                return normalized[..normalized.IndexOf('@')];
            }

            return normalized;
        }

        private string BuildUserPrincipalName(string accountName)
        {
            var suffix = !string.IsNullOrWhiteSpace(_activeDirectorySettings.UserPrincipalNameSuffix)
                ? _activeDirectorySettings.UserPrincipalNameSuffix
                : _activeDirectorySettings.Domain;

            return string.IsNullOrWhiteSpace(suffix)
                ? accountName
                : $"{accountName}@{suffix}";
        }

        private string BuildEmail(string accountName, string? emailFromClaims)
        {
            if (!string.IsNullOrWhiteSpace(emailFromClaims))
            {
                return emailFromClaims.Trim().ToLowerInvariant();
            }

            var domain = !string.IsNullOrWhiteSpace(_activeDirectorySettings.EmailDomain)
                ? _activeDirectorySettings.EmailDomain
                : !string.IsNullOrWhiteSpace(_activeDirectorySettings.UserPrincipalNameSuffix)
                    ? _activeDirectorySettings.UserPrincipalNameSuffix
                    : _activeDirectorySettings.Domain;

            return string.IsNullOrWhiteSpace(domain)
                ? accountName.Trim().ToLowerInvariant()
                : $"{accountName.Trim().ToLowerInvariant()}@{domain}";
        }

        private static string? NormalizeOptional(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        public async Task<BaseResponse<bool>> LogoutAsync(string userId)
        {
            var response = new BaseResponse<bool>();
            
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    throw new NotFoundException(nameof(ApplicationUser), userId);
                }

                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                await _userManager.UpdateAsync(user);
                await _signInManager.SignOutAsync();
                response.Success = true;
                response.Message = "Logout successful.";
                response.Data = true;
                _logger.LogInformation("User {UserId} logged out successfully", userId);
                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during logout for user {UserId}", userId);
                response.Success = false;
                response.Message = "An error occurred during logout.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        public async Task<BaseResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var response = new BaseResponse<LoginResponse>();

            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.Token);
                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    response.Success = false;
                    response.Message = "Invalid token.";
                    return response;
                }

                var user = await _userManager.FindByIdAsync(userId);

                if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                {
                    response.Success = false;
                    response.Message = "Invalid or expired refresh token.";
                    return response;
                }

                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
                await _userManager.UpdateAsync(user);

                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = roles.ToList();

                response.Success = true;
                response.Message = "Token refreshed successfully.";
                response.Data = new LoginResponse
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    TokenExpiration = DateTime.UtcNow.AddHours(1),
                    User = userDto
                };

                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh for user {UserId}", request.Token);
                response.Success = false;
                response.Message = "An error occurred during token refresh.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
        public async Task<BaseResponse<UserDto>> RegisterAsync(RegisterRequest request)
        {
            var response = new BaseResponse<UserDto>();

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    response.Success = false;
                    response.Message = "Email is already registered.";
                    return response;
                }

                var user = new ApplicationUser
                {
                    NetworkUserName = request.Email.Contains('@') ? request.Email[..request.Email.IndexOf('@')] : request.Email,
                    UserName = request.Email,
                    Email = request.Email,
                    FullName = request.FullName,
                    Department = request.Department,
                    Position = request.Position,
                    RegistrationNumber = request.RegistrationNumber,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    response.Success = false;
                    response.Message = "User registration failed.";
                    response.Errors = result.Errors.Select(e => e.Description).ToList();
                    return response;
                }

                await _userManager.AddToRoleAsync(user, "User");
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = new List<string> { "User" };

                response.Success = true;
                response.Message = "User registered successfully.";
                response.Data = userDto;
                return response;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for email {Email}", request.Email);
                response.Success = false;
                response.Message = "An error occurred during user registration.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }
    }
}
