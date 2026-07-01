using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Queries.ListUsers
{
    public class ListUsersQuery : IRequest<BaseResponse<List<UserDto>>>
    {
    }
}
