using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;

namespace AGE.SignatureHub.Application.Contracts.Identity
{
    public interface IAuthService
    {
        Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<BaseResponse<LoginResponse>> RefreshTokenAsync(RefreshTokenRequest request);
        Task<BaseResponse<bool>> LogoutAsync(string userId);
        Task<BaseResponse<UserDto>> RegisterAsync(RegisterRequest request);
        Task<BaseResponse<UserDto>> GetCurrentUserAsync(string userId);
        Task<BaseResponse<bool>> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}