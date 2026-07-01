using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Queries.ListUsers
{
    public class ListUsersQueryHandler : IRequestHandler<ListUsersQuery, BaseResponse<List<UserDto>>>
    {
        private readonly IUserManagementService _userManagementService;

        public ListUsersQueryHandler(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        public async Task<BaseResponse<List<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
        {
            return new BaseResponse<List<UserDto>>
            {
                Success = true,
                Message = "Users listed successfully.",
                Data = await _userManagementService.ListAllAsync(cancellationToken)
            };
        }
    }
}
