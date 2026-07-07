using AGE.SignatureHub.Application.Contracts.Identity;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Document;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Entities;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.TransferDocumentDepartment
{
    public class TransferDocumentDepartmentCommandHandler : IRequestHandler<TransferDocumentDepartmentCommand, BaseResponse<DocumentDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserManagementService _userManagementService;
        private readonly IMapper _mapper;

        public TransferDocumentDepartmentCommandHandler(
            IUnitOfWork unitOfWork,
            IUserManagementService userManagementService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManagementService = userManagementService;
            _mapper = mapper;
        }

        public async Task<BaseResponse<DocumentDto>> Handle(TransferDocumentDepartmentCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var requestingUser = await _userManagementService.GetByIdAsync(request.TransferData.RequestingUserId, cancellationToken);
                var targetUser = await _userManagementService.GetByIdAsync(request.TransferData.TargetUserId, cancellationToken);

                var document = await _unitOfWork.Documents.GetAccessibleByIdWithAllRelationsAsync(
                    request.DocumentId,
                    request.TransferData.RequestingUserId,
                    requestingUser.Email,
                    requestingUser.Department,
                    cancellationToken);

                if (document is null)
                {
                    throw new NotFoundException(nameof(Document), request.DocumentId);
                }

                var requesterEmail = Normalize(requestingUser.Email);
                var targetEmail = Normalize(targetUser.Email);
                var isCreator = document.CreatedByUserId == request.TransferData.RequestingUserId;
                var requesterIsParticipant = document.SignatureFlows.Any(sf =>
                    sf.Signers.Any(s => Normalize(s.Email) == requesterEmail));

                if (!isCreator && !requesterIsParticipant)
                {
                    throw new BusinessException("Apenas o criador ou participantes do documento podem solicitar a movimentação entre departamentos.");
                }

                var targetIsParticipant = document.SignatureFlows.Any(sf =>
                    sf.Signers.Any(s => Normalize(s.Email) == targetEmail));

                if (!targetIsParticipant)
                {
                    throw new BusinessException("O usuário de destino precisa participar do fluxo do documento para receber a movimentação.");
                }

                if (string.IsNullOrWhiteSpace(targetUser.Department))
                {
                    throw new BusinessException("O usuário de destino não possui departamento vinculado.");
                }

                var currentDepartment = document.OwningDepartment.Trim();
                var targetDepartment = targetUser.Department.Trim();

                if (string.Equals(currentDepartment, targetDepartment, StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessException("O documento já pertence ao departamento do usuário de destino.");
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                document.TransferOwnershipToDepartment(targetDepartment);
                await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);

                var auditLog = new AuditLog(
                    action: "DOCUMENT_DEPARTMENT_TRANSFERRED",
                    details: $"Document '{document.Title}' moved from department '{currentDepartment}' to '{targetDepartment}' by '{requestingUser.FullName}' ({requestingUser.NetworkUserName}) targeting '{targetUser.FullName}' ({targetUser.NetworkUserName}). Reason: {request.TransferData.Reason.Trim()}",
                    ipAddress: request.TransferData.IpAddress,
                    userAgent: request.TransferData.UserAgent,
                    documentId: document.Id,
                    userId: request.TransferData.RequestingUserId);

                await _unitOfWork.AuditLogs.AddAsync(auditLog, cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return new BaseResponse<DocumentDto>
                {
                    Success = true,
                    Message = "Document department transferred successfully.",
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
