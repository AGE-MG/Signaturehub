using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.DTOs.Common;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Users.Commands.UpdateMyProfile
{
    public class UpdateMyProfileCommand : IRequest<BaseResponse<UserDto>>
    {
        public string CurrentUserId { get; set; } = string.Empty;
        public UpdateProfileDto Data { get; set; } = new();
    }
}
