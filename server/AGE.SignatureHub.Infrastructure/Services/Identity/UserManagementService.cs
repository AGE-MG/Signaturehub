using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AGE.SignatureHub.Infrastructure.Services.Identity
{
    public class UserManagementService : IUserManagementService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IMapper _mapper;

        public UserManagementService(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
        }

        public async Task<List<UserDto>> ListAllAsync(CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users
                .OrderBy(u => u.FullName)
                .ToListAsync(cancellationToken);

            var result = new List<UserDto>(users.Count);
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var dto = _mapper.Map<UserDto>(user);
                dto.Roles = roles.ToList();
                result.Add(dto);
            }

            return result;
        }

        public async Task<UserDto> UpdateMyProfileAsync(string userId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
            {
                throw new NotFoundException(nameof(ApplicationUser), userId);
            }

            var fullName = dto.FullName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new BusinessException("Nome completo é obrigatório.");
            }

            user.FullName = fullName;
            user.Department = dto.Department?.Trim();
            user.Position = dto.Position?.Trim();
            user.RegistrationNumber = dto.RegistrationNumber?.Trim();

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                throw new BusinessException(string.Join(" | ", updateResult.Errors.Select(e => e.Description)));
            }

            var roles = await _userManager.GetRolesAsync(user);
            var updated = _mapper.Map<UserDto>(user);
            updated.Roles = roles.ToList();
            return updated;
        }

        public async Task<UserDto> UpdateUserRolesAsync(Guid userId, List<string> roles, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new NotFoundException(nameof(ApplicationUser), userId);
            }

            var newRoles = (roles ?? new List<string>())
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Select(r => r.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var role in newRoles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    throw new BusinessException($"Role '{role}' não existe.");
                }
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                throw new BusinessException(string.Join(" | ", removeResult.Errors.Select(e => e.Description)));
            }

            if (newRoles.Count > 0)
            {
                var addResult = await _userManager.AddToRolesAsync(user, newRoles);
                if (!addResult.Succeeded)
                {
                    throw new BusinessException(string.Join(" | ", addResult.Errors.Select(e => e.Description)));
                }
            }

            var updatedRoles = await _userManager.GetRolesAsync(user);
            var updated = _mapper.Map<UserDto>(user);
            updated.Roles = updatedRoles.ToList();
            return updated;
        }

        public async Task DeleteUserAsync(Guid userId, string? currentUserId, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(currentUserId, userId.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessException("Não é permitido remover o próprio usuário.");
            }

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null)
            {
                throw new NotFoundException(nameof(ApplicationUser), userId);
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                throw new BusinessException(string.Join(" | ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
