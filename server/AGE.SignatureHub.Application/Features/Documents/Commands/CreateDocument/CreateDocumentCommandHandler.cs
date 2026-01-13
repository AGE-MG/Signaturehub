using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly ICryptographyService _cryptographyService;
        private readonly IMapper _mapper;

        public CreateDocumentCommandHandler(IUnitOfWork unitOfWork, IStorageService storageService, ICryptographyService cryptographyService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _cryptographyService = cryptographyService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<DocumentDto>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<DocumentDto>();

            try
            {
                var validator = new CreateDocumentCommandValidator();

                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    response.Success = false;
                    response.Message = "Validation errors occurred.";
                    response.Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return response;
                }

                request.FileStream.Position = 0;
                var contentHash = await _cryptographyService.ComputeHashAsync(request.FileStream, cancellationToken);

                request.FileStream.Position = 0;
                var FileExtension = Path.GetExtension(request.FileName);
                var mimeType = GetMimeType(FileExtension);
                var storagePath = await _storageService.UploadFileAsync(request.FileStream, request.FileName, mimeType, cancellationToken);

                var document = new Document(
                    fileName: Guid.NewGuid().ToString() + FileExtension,
                    originalFileName: request.FileName,
                    fileExtension: FileExtension,
                    fileSizeInBytes: request.FileStream.Length,
                    storagePath: storagePath,
                    contentHash: contentHash,
                    mimeType: mimeType,
                    title: request.DocumentData.Title,
                    description: request.DocumentData.Description,
                    createdByUserId: request.DocumentData.CreatedByUserId,
                    expiresAt: request.DocumentData.ExpiresAt
                );

                await _unitOfWork.Documents.AddAsync(document, cancellationToken);

                var auditLog = new AuditLog(
                    action: "DOCUMENT_CREATED",
                    details: $"Document '{document.Title}' created by user '{document.CreatedByUserId}'.",
                    ipAddress: "0.0.0.0",
                    userAgent: "System",
                    documentId: document.Id,
                    userId: request.DocumentData.CreatedByUserId
                );

                await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                response.Success = true;
                response.Message = "Document created successfully.";
                response.Data = _mapper.Map<DocumentDto>(document);
                return response;
            }
            catch (System.Exception ex)
            {
                response.Success = false;
                response.Message = "An error occurred while creating the document.";
                response.Errors = new List<string> { ex.Message };
                return response;
            }
        }

        private string GetMimeType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                _ => "application/octet-stream",
            };
        }
    }
}