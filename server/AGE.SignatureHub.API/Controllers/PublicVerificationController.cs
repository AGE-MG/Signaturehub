using AGE.SignatureHub.Application.Configuration;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Public;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AGE.SignatureHub.API.Controllers
{
    [ApiController]
    [Route("api/v1/public/verification")]
    [AllowAnonymous]
    public class PublicVerificationController : ControllerBase
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly ApplicationSettings _settings;

        public PublicVerificationController(
            IDocumentRepository documentRepository,
            IOptions<ApplicationSettings> settings)
        {
            _documentRepository = documentRepository;
            _settings = settings.Value;
        }

        [HttpGet("documents/{documentId:guid}")]
        [ProducesResponseType(typeof(PublicDocumentVerificationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDocumentVerification(Guid documentId, [FromQuery] int? version, CancellationToken cancellationToken)
        {
            var document = await _documentRepository.GetByIdWithAllRelationsAsync(documentId, cancellationToken);
            if (document is null)
            {
                return NotFound();
            }

            var selectedVersion = version.HasValue
                ? document.Versions.FirstOrDefault(v => v.VersionNumber == version.Value)
                : document.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

            if (selectedVersion is null)
            {
                return NotFound();
            }

            var baseUrl = (_settings.FrontendUrl ?? _settings.ApplicationUrl ?? string.Empty).TrimEnd('/');
            var verificationUrl = $"{baseUrl}/verification/documents/{document.Id}?version={selectedVersion.VersionNumber}";

            var response = new PublicDocumentVerificationDto
            {
                DocumentId = document.Id,
                VersionNumber = selectedVersion.VersionNumber,
                Title = document.Title,
                OriginalFileName = document.OriginalFileName,
                ContentHash = selectedVersion.ContentHash,
                Status = document.Status,
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                VerificationUrl = verificationUrl,
                SignedSigners = document.SignatureFlows
                    .SelectMany(flow => flow.Signers)
                    .Where(signer => signer.SignedAt.HasValue)
                    .OrderBy(signer => signer.SignedAt)
                    .Select(signer => new PublicSignedSignerDto
                    {
                        SignerId = signer.Id,
                        Name = signer.Name,
                        Email = signer.Email,
                        SignatureType = signer.SignatureType,
                        SignedAt = signer.SignedAt
                    })
                    .ToList()
            };

            return Ok(response);
        }
    }
}
