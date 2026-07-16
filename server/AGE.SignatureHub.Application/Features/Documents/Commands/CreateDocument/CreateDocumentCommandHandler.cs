using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Application.DTOs.Notifications;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, BaseResponse<DocumentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;
        private readonly ICryptographyService _cryptographyService;
        private readonly IUserManagementService _userManagementService;
        private readonly IMapper _mapper;
        private readonly IDocumentNotificationDispatcher _notifications;

        public CreateDocumentCommandHandler(
            IUnitOfWork unitOfWork,
            IStorageService storageService,
            ICryptographyService cryptographyService,
            IUserManagementService userManagementService,
            IMapper mapper,
            IDocumentNotificationDispatcher notifications)
        {
            _unitOfWork = unitOfWork;
            _storageService = storageService;
            _cryptographyService = cryptographyService;
            _userManagementService = userManagementService;
            _mapper = mapper;
            _notifications = notifications;
        }

        public async Task<BaseResponse<DocumentDto>> Handle(CreateDocumentCommand request, CancellationToken cancellationToken)
        {
            request.FileStream.Position = 0;
            var contentHash = await _cryptographyService.ComputeHashAsync(request.FileStream, cancellationToken);

            request.FileStream.Position = 0;
            var FileExtension = Path.GetExtension(request.FileName);
            var mimeType = GetMimeType(FileExtension);
            var storagePath = await _storageService.UploadFileAsync(request.FileStream, request.FileName, mimeType, cancellationToken);
            var creator = await _userManagementService.GetByIdAsync(request.DocumentData.CreatedByUserId, cancellationToken);

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
                owningDepartment: creator.Department ?? string.Empty,
                isConfidential: request.DocumentData.IsConfidential,
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

            await _notifications.EnqueueAsync(new DocumentEventNotification
            {
                EventType = "document.created",
                DocumentId = document.Id,
                DocumentTitle = document.Title,
                ActorUserId = document.CreatedByUserId
            }, cancellationToken);

            return new BaseResponse<DocumentDto>
            {
                Success = true,
                Message = "Document created successfully.",
                Data = _mapper.Map<DocumentDto>(document)
            };
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
