using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Commands.UpdateUserRoles
{
    public class UpdateUserRolesCommand : IRequest<BaseResponse<UserDto>>
    {
        public Guid UserId { get; set; }
        public UpdateUserRolesDto Data { get; set; } = new();
    }
}
