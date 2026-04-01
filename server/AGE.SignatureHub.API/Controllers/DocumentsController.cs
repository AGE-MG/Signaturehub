using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.Features.Documents.Commands.CreateDocument;
using AGE.SignatureHub.Application.Features.Documents.Queries.DownloadDocument;
using AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentById;
using AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentByStatus;
using AGE.SignatureHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class DocumentsController : ControllerBase
    {
        private readonly ILogger<DocumentsController> _logger;
        private readonly IMediator _mediator;

        public DocumentsController(ILogger<DocumentsController> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new document for signing.
        /// </summary>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateDocument(IFormFile file, [FromForm] CreateDocumentDto documentData, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File is required.");
            }

            using var stream = file.OpenReadStream();

            var command = new CreateDocumentCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                DocumentData = documentData
            };

            var result = await _mediator.Send(command, cancellationToken);

            if (result.Success)
            {
                return CreatedAtAction(nameof(GetDocumentByIdQuery), new { id = result.Data.Id }, result);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

        /// <summary>
        /// Retrieves a document by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentById(Guid id, CancellationToken cancellationToken)
        {
            var query = new GetDocumentByIdQuery { DocumentId = id };
            var result = await _mediator.Send(query, cancellationToken);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result.Errors);
            }


        }

        /// <summary>
        /// Gets documents by status.
        /// </summary>
        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentsByStatus(DocumentStatus status, CancellationToken cancellationToken)
        {
            var query = new GetDocumentByStatusQuery { Status = status };
            var result = await _mediator.Send(query, cancellationToken);

            return Ok(result);
        }

        /// <summary>
        /// Downloads the original document or the signed document.
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocument(Guid id, [FromQuery] int? version, CancellationToken cancellationToken)
        {
            var query = new DownloadDocumentQuery
            {
                DocumentId = id,
                VersionNumber = version
            };

            var result = await _mediator.Send(query, cancellationToken);

            return File(result.FileStream, result.ContentType, result.FileName);
        }
    }
}