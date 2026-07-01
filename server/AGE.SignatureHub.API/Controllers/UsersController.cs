using System.Security.Claims;
using AGE.SignatureHub.Application.DTOs.Auth;
using AGE.SignatureHub.Application.Features.Users.Commands.DeleteUser;
using AGE.SignatureHub.Application.Features.Users.Commands.UpdateMyProfile;
using AGE.SignatureHub.Application.Features.Users.Commands.UpdateUserRoles;
using AGE.SignatureHub.Application.Features.Users.Queries.ListUsers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class UsersController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IMediator mediator,
            ILogger<UsersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Administrator")]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> ListAll(CancellationToken cancellationToken)
        {
            var query = new ListUsersQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResponse(result);
        }

        [HttpPut("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateProfileRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var command = new UpdateMyProfileCommand
            {
                CurrentUserId = userId,
                Data = new UpdateProfileDto
                {
                    FullName = request.FullName,
                    Department = request.Department,
                    Position = request.Position,
                    RegistrationNumber = request.RegistrationNumber
                }
            };

            var result = await _mediator.Send(command);
            return HandleResponse(result);
        }

        [HttpPut("{userId:guid}/roles")]
        [Authorize(Roles = "Admin,Administrator")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateRoles(Guid userId, [FromBody] UpdateRolesRequest request)
        {
            var command = new UpdateUserRolesCommand
            {
                UserId = userId,
                Data = new UpdateUserRolesDto
                {
                    Roles = request.Roles
                }
            };

            var result = await _mediator.Send(command);
            return HandleResponse(result);
        }

        [HttpDelete("{userId:guid}")]
        [Authorize(Roles = "Admin,Administrator")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(Guid userId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new DeleteUserCommand
            {
                UserId = userId,
                CurrentUserId = currentUserId
            };

            var result = await _mediator.Send(command);
            if (!result.Success)
            {
                _logger.LogWarning("Falha ao remover usuário {UserId}: {Errors}", userId, string.Join(" | ", result.Errors));
            }

            return HandleNoContentResponse(result);
        }

        public class UpdateProfileRequest
        {
            public string FullName { get; set; } = string.Empty;
            public string? Department { get; set; }
            public string? Position { get; set; }
            public string? RegistrationNumber { get; set; }
        }

        public class UpdateRolesRequest
        {
            public List<string> Roles { get; set; } = new();
        }
    }
}
