using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Domain.Enums;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Signers.Commands.RejectDocument
{
    public class RejectDocumentCommandHandler : IRequestHandler<RejectDocumentCommand, BaseResponse<SignerDto>>
    {

        private readonly IUnitOfWork _unitOfWork;
        IEmailService _emailService;
        IWebhookService _webhookService;
        IMapper _mapper;

        public RejectDocumentCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, IWebhookService webhookService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _webhookService = webhookService;
            _mapper = mapper;
        }
        public async Task<BaseResponse<SignerDto>> Handle(RejectDocumentCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<SignerDto>();

            try
            {
                if (string.IsNullOrWhiteSpace(request.RejectData.Reason))
                {
                    response.Success = false;
                    response.Message = "Rejection reason is required.";
                    return response;
                }

                var signer = await _unitOfWork.Signers.GetByIdWithFlowAndDocumentAsync(request.RejectData.SignerId, cancellationToken);
                if (signer == null)
                {
                    throw new NotFoundException(nameof(signer), request.RejectData.SignerId);
                }
                var flow = signer.SignatureFlow;
                var document = flow.Document;

                if (signer.Status != SignatureStatus.Pending)
                {
                    throw new BusinessException("Only pending signers can reject the document.");
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                signer.Reject(request.RejectData.Reason);

                document.UpdateStatus(DocumentStatus.Rejected);

                foreach (var otherSigner in flow.Signers.Where(s => s.Status == SignatureStatus.Pending))
                {
                    if (otherSigner.Id != signer.Id)
                    {
                        otherSigner.Cancel();
                    }
                }

                await _unitOfWork.Signers.UpdateAsync(signer, cancellationToken);
                await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);

                var auditLog = new AuditLog(
                    action: "DOCUMENT_REJECTED",
                    details: $"Signer {signer.Name} ({signer.Email}) rejected the document '{document.Title}' with reason: {request.RejectData.Reason}",
                    ipAddress: "0.0.0.0",
                    userAgent: "System",
                    documentId: document.Id,
                    signerId: signer.Id
                );

                await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                foreach (var participant in flow.Signers)
                {
                    await _emailService.SendSignatureRejectedAsync(participant.Email, participant.Name, document.Title, request.RejectData.Reason, cancellationToken);
                }

                await _webhookService.SendWebhookAsync("signature.rejected", JsonSerializer.Serialize(new
                {
                    DocumentId = document.Id,
                    FlowId = flow.Id,
                    SignerId = signer.Id,
                    DocumentTitle = document.Title,
                    Reason = request.RejectData.Reason
                }), cancellationToken);

                response.Success = true;
                response.Message = "Document rejected successfully.";
                response.Data = _mapper.Map<SignerDto>(signer);
                return response;
            }
            catch (System.Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                response.Success = false;
                response.Message = $"An error occurred while rejecting the document: {ex.Message}";
                response.Errors = new List<string> { ex.Message };
                
                return response;
            }
        }
    }
}