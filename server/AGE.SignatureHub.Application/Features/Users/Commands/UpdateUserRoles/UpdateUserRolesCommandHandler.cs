using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Commands.UpdateUserRoles
{
    public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand, BaseResponse<UserDto>>
    {
        private readonly IUserManagementService _userManagementService;

        public UpdateUserRolesCommandHandler(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public async Task<BaseResponse<UserDto>> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
        {
            return new BaseResponse<UserDto>
            {
                Success = true,
                Message = "User roles updated successfully.",
                Data = await _userManagementService.UpdateUserRolesAsync(request.UserId, request.Data.Roles, cancellationToken)
            };
        }
    }
}
