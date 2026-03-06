using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using AGE.SignatureHub.Application.Features.SignatureFlows.Commands.CreateSignatureFlow;
using AGE.SignatureHub.Application.Features.SignatureFlows.queries.GetFlowsByDocument;
using AGE.SignatureHub.Application.Features.SignatureFlows.queries.GetSignatureFlowById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SignatureFlowController : ControllerBase
    {
        private readonly ILogger<SignatureFlowController> _logger;
        private readonly IMediator _mediator;

        public SignatureFlowController(ILogger<SignatureFlowController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Starts a new signature flow for a document.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(SignatureFlowDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> StartSignatureFlow([FromBody] CreateSignatureFlowDto flowData, CancellationToken cancellationToken)
        {
            var command = new CreateSignatureFlowCommand
            {
                FlowData = flowData
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }
            
            return CreatedAtAction(nameof(CreateSignatureFlowCommand), new { id = result.Data.Id }, result);
        }

        /// <summary>
        /// Gets a signature flow by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SignatureFlowDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSignatureFlowById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetSignatureFlowByIdQuery { Id = id };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                return NotFound(result.Errors);
            }
            return Ok(result);
        }

        /// <summary>
        /// Gets all signature flows for a specific document.
        /// </summary> 
        [HttpGet("document/{documentId}")]
        [ProducesResponseType(typeof(List<SignatureFlowDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSignatureFlowsByDocumentId(Guid documentId, CancellationToken cancellationToken)
        {
            var query = new GetFlowsByDocumentQuery { DocumentId = documentId };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                return NotFound(result.Errors);
            }
            return Ok(result);
        }
    }
}