using AGE.SignatureHub.Application.DTOs.Auth;

namespace AGE.SignatureHub.Application.Contracts.Identity
{
    public interface IUserManagementService
    {
        Task<List<UserDto>> ListAllAsync(CancellationToken cancellationToken = default);
        Task<UserDto> UpdateMyProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
        Task<UserDto> UpdateUserRolesAsync(Guid userId, List<string> roles, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(Guid userId, string? currentUserId, CancellationToken cancellationToken = default);
    }
}
