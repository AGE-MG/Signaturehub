using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Signer;
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
            //TODO: Implement GetPendingSignaturesByEmailQuery and handle the request
            return NotFound();
        }
    }
}