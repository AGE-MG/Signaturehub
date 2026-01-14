using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.SignatureFlow;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.CreateSignatureFlow
{
    public class CreateSignatureFlowCommandHandler : IRequestHandler<CreateSignatureFlowCommand, BaseResponse<SignatureFlowDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IWebhookService _webhookService;
        private readonly IMapper _mapper;
        public CreateSignatureFlowCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService, IWebhookService webhookService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _webhookService = webhookService;
            _mapper = mapper;
        }
        
        public async Task<BaseResponse<SignatureFlowDto>> Handle(CreateSignatureFlowCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<SignatureFlowDto>();

            try
            {
                var validator = new CreateSignatureFlowCommandValidator();
                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    response.Success = false;
                    response.Message = "Validation errors occurred.";
                    response.Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return response;
                }

                var document = await _unitOfWork.Documents.GetByIdAsync(request.FlowData.DocumentId, cancellationToken);
                if (document == null)
                {
                    throw new NotFoundException(nameof(Document), request.FlowData.DocumentId);
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var signatureFlow = new SignatureFlow(
                    documentId: request.FlowData.DocumentId,
                    flowName: request.FlowData.FlowName,
                    flowType: request.FlowData.FlowType
                );

                foreach (var signerDto in request.FlowData.Signers)
                {
                    var signer = new Signer(
                        signatureFlowId: signatureFlow.Id,
                        name: signerDto.Name,
                        email: signerDto.Email,
                        document: signerDto.Document,
                        role: signerDto.Role,
                        signOrder: signerDto.SignOrder
                    );
                    signatureFlow.AddSigner(signer);
                }

                document.AddSignatureFlow(signatureFlow);

                await _unitOfWork.SignatureFlows.AddAsync(signatureFlow, cancellationToken);
                await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);

                var auditLog = new AuditLog(
                    action: "SIGNATURE_FLOW_CREATED",
                    details: $"Signature flow '{signatureFlow.FlowName}' created with {signatureFlow.Signers.Count} signers.",
                    ipAddress: "0.0.0.0",
                    userAgent: "System",
                    documentId: document.Id
                );

                await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                await SendSignatureRequestNotification(signatureFlow, document, cancellationToken);

                await _webhookService.SendWebhookAsync("signature.requested", JsonSerializer.Serialize(new      
                {
                    DocumentId = document.Id,
                    FlowId = signatureFlow.Id,
                    DocumentTitle = document.Title
                }), cancellationToken);

                response.Success = true;
                response.Message = "Signature flow created successfully.";
                response.Data = _mapper.Map<SignatureFlowDto>(signatureFlow);
                return response;
            }
            catch (System.Exception ex )
            {
                


                
                throw;
            }
        }
    }
}