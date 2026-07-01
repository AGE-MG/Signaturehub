using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Commands.DeleteUser
{
    public class DeleteUserCommand : IRequest<BaseResponse<bool>>
    {
        public Guid UserId { get; set; }
        public string? CurrentUserId { get; set; }
    }
}
