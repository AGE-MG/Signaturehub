using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDateRange;
using AGE.SignatureHub.Application.Features.AuditLogs.Queries.GetAuditLogsByDocument;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuditLogsController : ControllerBase
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
            var query = new GetAuditLogsByDocumentQuery
            {
                DocumentId = documentId
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Get audit logs by period of time.
        /// </summary>
        [HttpGet("date-range")]
        [ProducesResponseType(typeof(List<AuditLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAuditLogsByDateRange([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
        {
            var query = new GetAuditLogsByDateRangeQuery
            {
                StartDate = startDate,
                EndDate = endDate
            };
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
    }
}