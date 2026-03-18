using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ITokenService tokenService,
            IMapper mapper,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<BaseResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponse<UserDto>> GetCurrentUserAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            var response = new BaseResponse<LoginResponse>();

            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    response.Success = false;
                    response.Message = "Invalid email or password.";
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
                        _logger.LogWarning("User {Email} account locked out.", request.Email);
                        response.Success = false;
                        response.Message = "Account locked due to multiple failed login attempts. Please try again later.";
                        return response;
                    }

                    response.Success = false;
                    response.Message = "Invalid email or password.";
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

                _logger.LogInformation("User {Email} logged in successfully", request.Email);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Email}", request.Email);
                response.Success = false;
                response.Message = "An error occurred during login.";
                return response;
            }
        }

        public Task<BaseResponse<bool>> LogoutAsync(string userId)
        {
            var response = new BaseResponse<bool>();
            
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

        public Task<BaseResponse<UserDto>> RegisterAsync(RegisterRequest request)
        {
            throw new NotImplementedException();
        }
    }
}