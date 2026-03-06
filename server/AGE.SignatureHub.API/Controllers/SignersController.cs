using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Signer;
using AGE.SignatureHub.Application.Features.Documents.Queries.GetPendingSignaturesByEmail;
using AGE.SignatureHub.Application.Features.Signers.Commands.RejectDocument;
using AGE.SignatureHub.Application.Features.Signers.Commands.SignDocument;
using AGE.SignatureHub.Application.Features.Signers.Queries.GetSignerById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SignersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SignersController> _logger;
        public SignersController(IMediator mediator, ILogger<SignersController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// get pending signaturees by email
        /// </summary>
        [HttpGet("pending/{email}")]
        [ProducesResponseType(typeof(List<SignerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingSignaturesByEmail(string email, CancellationToken cancellationToken)
        {
            var query = new GetPendingSignaturesByEmailQuery
            {
                Email = email
            };

            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// sign a document        
        /// </summary>
        [HttpPost("sign")]
        [ProducesResponseType(typeof(SignerDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SignDocument([FromBody] SignDocumentDto signData, CancellationToken cancellationToken)
        {
            signData.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            signData.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var command = new SignDocumentCommand
            {
                SignData = signData
            };
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result);
        }

        /// <summary>
        /// Reject a document signature
        /// </summary>
        [HttpPost("reject")]
        [ProducesResponseType(typeof(SignerDto),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectDocumentSignature([FromBody] RejectDocumentDto rejectData, CancellationToken cancellationToken)
        {
            var command = new RejectDocumentCommand
            {
                RejectData = rejectData,
            };
            var result = await _mediator.Send(command, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(result.Errors);
            }
            return Ok(result);
        }

        /// <summary>
        /// Gets a signer by ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(SignerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSignerById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetSignerByIdQuery { SignerId = id };

            var result = await _mediator.Send(query, cancellationToken);

            if (!result.Success)
            {
                return NotFound(result.Errors);
            }
            return Ok(result);
        }
    }
}