using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.DTOs.Notifications;
using AGE.SignatureHub.Application.Features.Documents.Commands.CreateDocument;
using AGE.SignatureHub.Application.Features.Documents.Commands.TransferDocumentDepartment;
using AGE.SignatureHub.Application.Features.Documents.Queries.DownloadDocument;
using AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentById;
using AGE.SignatureHub.Application.Features.Documents.Queries.GetDocumentByStatus;
using AutoMapper;
using AGE.SignatureHub.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class DocumentsController : ApiControllerBase
    {
        private readonly ILogger<DocumentsController> _logger;
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagementService;
        private readonly IMapper _mapper;
        private readonly IDocumentNotificationDispatcher _notifications;

        public DocumentsController(ILogger<DocumentsController> logger, IMediator mediator, IUnitOfWork unitOfWork, IUserManagementService userManagementService, IMapper mapper, IDocumentNotificationDispatcher notifications)
        {
            _logger = logger;
            _mediator = mediator;
            _unitOfWork = unitOfWork;
            _userManagementService = userManagementService;
            _mapper = mapper;
            _notifications = notifications;
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            // O criador deve vir sempre da identidade autenticada, nunca do corpo enviado pelo cliente.
            documentData.CreatedByUserId = parsedUserId;

            using var stream = file.OpenReadStream();

            var command = new CreateDocumentCommand
            {
                FileStream = stream,
                FileName = file.FileName,
                DocumentData = documentData
            };

            var result = await _mediator.Send(command, cancellationToken);
            return HandleCreatedResponse(result, nameof(GetDocumentById), new { id = result.Data?.Id });
        }

        /// <summary>
        /// Gets documents for the authenticated user, with optional status filter.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocuments([FromQuery] DocumentStatus? status, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManagementService.GetByIdAsync(parsedUserId, cancellationToken);
            var documents = await _unitOfWork.Documents.GetAccessibleDocumentsAsync(
                parsedUserId,
                currentUser.Email,
                currentUser.Department,
                status,
                cancellationToken);

            var result = _mapper.Map<List<DocumentDto>>(documents);
            return Ok(result);
        }

        /// <summary>
        /// Retrieves a document by its ID.
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentById(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManagementService.GetByIdAsync(parsedUserId, cancellationToken);
            var query = new GetDocumentByIdQuery
            {
                DocumentId = id,
                RequestingUserId = parsedUserId,
                RequestingUserEmail = currentUser.Email,
                RequestingUserDepartment = currentUser.Department
            };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResponse(result);
        }

        /// <summary>
        /// Gets documents by status.
        /// </summary>
        [HttpGet("by-status/{status}")]
        [ProducesResponseType(typeof(IEnumerable<DocumentDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDocumentsByStatus(DocumentStatus status, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManagementService.GetByIdAsync(parsedUserId, cancellationToken);
            var query = new GetDocumentByStatusQuery
            {
                Status = status,
                RequestingUserId = parsedUserId,
                RequestingUserEmail = currentUser.Email,
                RequestingUserDepartment = currentUser.Department
            };
            var result = await _mediator.Send(query, cancellationToken);
            return HandleResponse(result);
        }

        /// <summary>
        /// Downloads the original document or the signed document.
        /// </summary>
        [HttpGet("{id}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DownloadDocument(Guid id, [FromQuery] int? version, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManagementService.GetByIdAsync(parsedUserId, cancellationToken);
            var query = new DownloadDocumentQuery
            {
                DocumentId = id,
                VersionNumber = version,
                RequestingUserId = parsedUserId,
                RequestingUserEmail = currentUser.Email,
                RequestingUserDepartment = currentUser.Department
            };

            var result = await _mediator.Send(query, cancellationToken);

            return File(result.FileStream, result.ContentType, result.FileName);
        }

        /// <summary>
        /// Deletes a draft document created by the authenticated user.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDocument(Guid id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            var currentUser = await _userManagementService.GetByIdAsync(parsedUserId, cancellationToken);
            var document = await _unitOfWork.Documents.GetAccessibleByIdWithAllRelationsAsync(
                id,
                parsedUserId,
                currentUser.Email,
                currentUser.Department,
                cancellationToken);
            if (document is null)
            {
                return NotFound(new[] { "Documento não encontrado." });
            }

            if (document.CreatedByUserId != parsedUserId)
            {
                return Forbid();
            }

            if (document.Status != DocumentStatus.Draft)
            {
                return BadRequest(new[] { "Apenas documentos em rascunho podem ser excluídos." });
            }

            document.MarkAsDeleted();

            foreach (var flow in document.SignatureFlows)
            {
                flow.MarkAsDeleted();

                foreach (var signer in flow.Signers)
                {
                    signer.MarkAsDeleted();
                }
            }

            await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _notifications.EnqueueAsync(new DocumentEventNotification
            {
                EventType = "document.deleted",
                DocumentId = document.Id,
                DocumentTitle = document.Title,
                ActorUserId = parsedUserId,
                RecipientEmails = document.SignatureFlows
                    .SelectMany(flow => flow.Signers)
                    .Select(signer => signer.Email)
                    .Where(email => !string.IsNullOrWhiteSpace(email))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            }, cancellationToken);

            return NoContent();
        }

        /// <summary>
        /// Transfers the document ownership department to the target participant department.
        /// </summary>
        [HttpPost("{id}/transfer-department")]
        [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TransferDepartment(Guid id, [FromBody] TransferDocumentDepartmentDto transferData, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId) || !Guid.TryParse(userId, out var parsedUserId))
            {
                return Unauthorized();
            }

            transferData.RequestingUserId = parsedUserId;
            transferData.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "0.0.0.0";
            transferData.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

            var command = new TransferDocumentDepartmentCommand
            {
                DocumentId = id,
                TransferData = transferData
            };

            var result = await _mediator.Send(command, cancellationToken);
            return HandleResponse(result);
        }
    }
}
