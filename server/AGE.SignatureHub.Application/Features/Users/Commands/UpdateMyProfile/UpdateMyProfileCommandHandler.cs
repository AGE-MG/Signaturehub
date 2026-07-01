using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateMyProfileCommand, BaseResponse<UserDto>>
    {
        private readonly IUserManagementService _userManagementService;

        public UpdateMyProfileCommandHandler(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public async Task<BaseResponse<UserDto>> Handle(UpdateMyProfileCommand request, CancellationToken cancellationToken)
        {
            return new BaseResponse<UserDto>
            {
                Success = true,
                Message = "Profile updated successfully.",
                Data = await _userManagementService.UpdateMyProfileAsync(request.CurrentUserId, request.Data, cancellationToken)
            };
        }
    }
}
