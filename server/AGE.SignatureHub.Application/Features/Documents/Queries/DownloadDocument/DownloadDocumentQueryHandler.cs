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
            
            string storagePath;
            string fileName;

            if (request.VersionNumber.HasValue)
            {
                var version = document.Versions.FirstOrDefault(v => v.VersionNumber == request.VersionNumber.Value);

                if (version == null)
                {
                    throw new NotFoundException("DocumentVersion", request.VersionNumber.Value);
                }

                storagePath = version.StoragePath;
                fileName = $"{document.OriginalFileName}_v{version.VersionNumber}{document.FileExtension}";
            } 
            else
            {
                var latestVersion = document.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

                if (latestVersion == null)
                {
                    storagePath = document.StoragePath;
                    fileName = document.OriginalFileName;
                }
                else
                {
                    storagePath = latestVersion.StoragePath;
                    fileName = $"{document.OriginalFileName}_v{latestVersion.VersionNumber}{document.FileExtension}";
                }
            }

            var fileStream = await _storageService.DownloadFileAsync(storagePath, cancellationToken);

            return new DownloadDocumentResponse
            {
                FileStream = fileStream,
                FileName = fileName,
                ContentType = document.MimeType
            };
        }
    }
}