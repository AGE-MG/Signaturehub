using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Queries.DownloadDocument
{
    public class DownloadDocumentQueryHandler : IRequestHandler<DownloadDocumentQuery, DownloadDocumentResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        public DownloadDocumentQueryHandler(IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
        }


        public async Task<DownloadDocumentResponse> Handle(DownloadDocumentQuery request, CancellationToken cancellationToken)
        {
            var document = await _unitOfWork.Documents.GetByIdWithVersionsAsync(request.DocumentId, cancellationToken);

            if (document == null)
            {
                throw new NotFoundException(nameof(Document), request.DocumentId);
            }
            
            var response = new DownloadDocumentResponse
            {
                FileStream = new MemoryStream(), // Replace with actual file stream
                FileName = "example.pdf", // Replace with actual file name
                ContentType = "application/pdf" // Replace with actual content type
            };

            return await Task.FromResult(response);
        }
    }
}