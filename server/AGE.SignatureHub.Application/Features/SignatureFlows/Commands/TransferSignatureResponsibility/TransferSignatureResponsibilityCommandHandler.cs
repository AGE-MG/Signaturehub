using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Configuration;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AGE.SignatureHub.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.Commands.TransferSignatureResponsibility
{
    public class TransferSignatureResponsibilityCommandHandler : IRequestHandler<TransferSignatureResponsibilityCommand, BaseResponse<DocumentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;
        private readonly ApplicationSettings _settings;

        public TransferSignatureResponsibilityCommandHandler(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IMapper mapper,
            IOptions<ApplicationSettings> settings)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _mapper = mapper;
            _settings = settings.Value;
        }

        public async Task<BaseResponse<DocumentDto>> Handle(TransferSignatureResponsibilityCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var document = await _unitOfWork.Documents.GetByIdWithAllRelationsAsync(request.DocumentId, cancellationToken);
                if (document == null)
                {
                    throw new NotFoundException(nameof(Document), request.DocumentId);
                }

                var requesterEmail = Normalize(request.TransferData.RequestingUserEmail);

                // Só quem já assinou pode transferir a própria responsabilidade; pega a assinatura
                // mais recente do requisitante caso ele tenha múltiplos papéis no documento.
                var current = document.SignatureFlows
                    .SelectMany(flow => flow.Signers.Select(signer => new { flow, signer }))
                    .Where(x => x.signer.Status == SignatureStatus.Signed && Normalize(x.signer.Email) == requesterEmail)
                    .OrderByDescending(x => x.signer.SignedAt)
                    .FirstOrDefault();

                if (current == null)
                {
                    throw new BusinessException("Você não possui uma assinatura concluída neste documento para transferir.");
                }

                var newEmail = Normalize(request.TransferData.NewResponsibleEmail);
                if (newEmail == Normalize(current.signer.Email))
                {
                    throw new BusinessException("O novo responsável deve ser diferente do atual.");
                }

                var flow = current.flow;
                var previousSigner = current.signer;

                // Fluxos sequenciais/híbridos: reabrir uma etapa anterior à atual voltaria
                // CurrentStep para trás e misturaria o estado de etapas já concluídas depois
                // dela. Paralelo não tem essa noção de ordem, então não se aplica.
                if (flow.FlowType != FlowType.Parallel && previousSigner.SignOrder != flow.TotalSteps)
                {
                    throw new BusinessException("Só é possível transferir a responsabilidade da última etapa do fluxo.");
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var newSigner = new Signer(
                    signatureFlowId: flow.Id,
                    name: request.TransferData.NewResponsibleName,
                    email: request.TransferData.NewResponsibleEmail,
                    document: request.TransferData.NewResponsibleDocument,
                    role: previousSigner.Role,
                    signOrder: previousSigner.SignOrder);

                flow.AddSigner(newSigner);
                flow.UpdateCurrentStep(previousSigner.SignOrder);
                flow.ReopenForTransfer();

                document.UpdateStatus(DocumentStatus.PendingSignatures);

                await _unitOfWork.Signers.AddAsync(newSigner, cancellationToken);
                await _unitOfWork.SignatureFlows.UpdateAsync(flow, cancellationToken);
                await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);

                var auditLog = new AuditLog(
                    action: "RESPONSIBILITY_TRANSFERRED",
                    details: $"Signing responsibility for flow '{flow.FlowName}' transferred from '{previousSigner.Name}' ({previousSigner.Email}) to '{newSigner.Name}' ({newSigner.Email}).",
                    ipAddress: request.TransferData.IpAddress,
                    userAgent: request.TransferData.UserAgent,
                    documentId: document.Id,
                    signerId: newSigner.Id,
                    userId: request.TransferData.RequestingUserId);

                await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                try
                {
                    var signatureUrl = $"{_settings.BaseUrl}{_settings.SignatureUrlPath}/{newSigner.Id}?token={Uri.EscapeDataString(newSigner.InvitationToken)}";
                    await _emailService.SendSignatureRequestAsync(
                        newSigner.Email,
                        newSigner.Name,
                        document.Title,
                        signatureUrl,
                        cancellationToken);
                }
                catch
                {
                    // A transferência já foi persistida; falha de e-mail não deve reverter a operação.
                }

                return new BaseResponse<DocumentDto>
                {
                    Success = true,
                    Message = "Signing responsibility transferred successfully.",
                    Data = _mapper.Map<DocumentDto>(document)
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        private static string Normalize(string? value)
        {
            return value?.Trim().ToLowerInvariant() ?? string.Empty;
        }
    }
}
