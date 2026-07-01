using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, BaseResponse<bool>>
    {
        private readonly IUserManagementService _userManagementService;

        public DeleteUserCommandHandler(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public async Task<BaseResponse<bool>> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            await _userManagementService.DeleteUserAsync(request.UserId, request.CurrentUserId, cancellationToken);
            return new BaseResponse<bool>
            {
                Success = true,
                Message = "User removed successfully.",
                Data = true
            };
        }
    }
}
