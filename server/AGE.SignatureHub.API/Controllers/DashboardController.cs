using System.Security.Claims;
using AGE.SignatureHub.Application.DTOs.Dashboard;
using AGE.SignatureHub.Application.Features.Dashboard.Commands.MarkAllNotificationsAsRead;
using AGE.SignatureHub.Application.Features.Dashboard.Commands.MarkNotificationAsRead;
using AGE.SignatureHub.Application.Features.Dashboard.Queries.GetDashboardStats;
using AGE.SignatureHub.Application.Features.Dashboard.Queries.GetRecentDocuments;
using AGE.SignatureHub.Application.Features.Dashboard.Queries.GetUserNotifications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IMediator mediator, ILogger<DashboardController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get dashboard data.
        /// </summary>
        [HttpGet("stats")]
        [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetStats(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = new GetDashboardStatsQuery { UserIdPacket = Guid.Parse(userId) };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get recent documents.
        /// </summary>
        [HttpGet("recent-documents")]
        [ProducesResponseType(typeof(List<RecentDocumentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRecentDocuments(
            CancellationToken cancellationToken = default,
            [FromQuery] int count = 5
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = new GetRecentDocumentsQuery
            {
                UserIdPacket = Guid.Parse(userId),
                Count = count
            };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Get user notifications.
        /// </summary>
        [HttpGet("notifications")]
        [ProducesResponseType(typeof(List<NotificationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotifications(
            [FromQuery] bool unreadOnly = false,
            CancellationToken cancellationToken = default
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = new GetUserNotificationsQuery
            {
                UserIdPacket = Guid.Parse(userId),
                UnreadOnly = unreadOnly
            };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Mark a notification as read.
        /// </summary>
        [HttpPut("notifications/{notificationId}/read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkNotificationAsRead(
            [FromRoute] Guid notificationId,
            CancellationToken cancellationToken = default
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var command = new MarkNotificationAsReadCommand
            {
                NotificationId = notificationId
            };
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// mark all notifications as read.
        /// </summary>
        [HttpPut("notifications/mark-all-read")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> MarkAllNotificationsAsRead(
            CancellationToken cancellationToken = default
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var command = new MarkAllNotificationsAsReadCommand
            {
                UserIdPacket = Guid.Parse(userId)
            };
            var result = await _mediator.Send(command, cancellationToken);

            return Ok(result);
        }
    }
}