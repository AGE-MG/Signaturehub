using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDateRange;
using AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDocument;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class AuditLogsController : ApiControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuditLogsController> _logger;
        public AuditLogsController(IMediator mediator, ILogger<AuditLogsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get audit logs for a specific document.
        /// </summary>
        [HttpGet("document/{documentId}")]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogsByDocument(Guid documentId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var query = new GetAuditLogsByDocumentQuery
            {
                DocumentId = documentId,
                RequestingUserId = parsedUserId,
                RequestingUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                RequestingUserDepartment = User.FindFirstValue("Department")
            };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResponse(result);
        }

        /// <summary>
        /// Get audit logs by period of time.
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var query = new GetAuditLogsByDateRangeQuery
            {
                StartDate = startDate,
                EndDate = endDate,
                RequestingUserId = parsedUserId,
                RequestingUserEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty,
                RequestingUserDepartment = User.FindFirstValue("Department"),
                IsAdmin = User.IsInRole("Admin") || User.IsInRole("Administrator")
            };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResponse(result);
        }
    }
}
