using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.Contracts.Infrastructure;
using AGE.SignatureHub.Application.Contracts.Persistence;
using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.DTOs.Signer;
using AGE.SignatureHub.Application.Exceptions;
using AGE.SignatureHub.Domain.Enums;
using AGE.SignatureHub.Domain.ValueObjects;
using AutoMapper;
using MediatR;

namespace AGE.SignatureHub.Application.Features.Signers.Commands.SignDocument
{
    public class SignDocumentCommandHandler : IRequestHandler<SignDocumentCommand, BaseResponse<SignerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISignatureService _signatureService;
        private readonly IStorageService _storageService;
        private readonly IEmailService _emailService;
        private readonly IWebhookService _webhookService;
        private readonly IMapper _mapper;

        public SignDocumentCommandHandler(
            IUnitOfWork unitOfWork,
            ISignatureService signatureService,
            IStorageService storageService,
            IEmailService emailService,
            IWebhookService webhookService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _signatureService = signatureService;
            _storageService = storageService;
            _emailService = emailService;
            _webhookService = webhookService;
            _mapper = mapper;
        }
        
        public async Task<BaseResponse<SignerDto>> Handle(SignDocumentCommand request, CancellationToken cancellationToken)
        {
            var response = new BaseResponse<SignerDto>();

            try
            {
                var validator = new SignDocumentCommandValidator();
                var validationResult = await validator.ValidateAsync(request, cancellationToken);

                if (!validationResult.IsValid)
                {
                    response.Success = false;
                    response.Message = "Sign document request validation failed.";
                    response.Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                    return response;
                }

                var signer = await _unitOfWork.Signers.GetByIdWithFlowAndDocumentAsync(request.SignData.SignerId, cancellationToken);

                if (signer == null)
                {
                    throw new NotFoundException(nameof(signer), request.SignData.SignerId);
                }

                var flow = signer.SignatureFlow;
                var document = flow.Document;

                document.CheckAndUpdateExpiration();
                if (document.IsExpired())
                {
                    throw new BusinessException("The document has expired and can no longer be signed.");
                }

                if (!flow.CanSignerSign(signer.Id))
                {
                    throw new BusinessException("The signer is not authorized to sign the document at this time.");
                }

                await _unitOfWork.BeginTransactionAsync(cancellationToken);


                CertificateInfo certificateInfo = null;
                if (request.SignData.SignatureType == SignatureType.DigitalA1 ||
                    request.SignData.SignatureType == SignatureType.DigitalA3)
                {
                    certificateInfo = await _signatureService.ValidateCertificateAsync(request.SignData.CertificateData, cancellationToken);
                    if (certificateInfo.IsExpired())
                    {
                        throw new BusinessException("The provided digital certificate has expired.");
                    }
                }

                var documentStream = await _storageService.DownloadFileAsync(document.StoragePath, cancellationToken);

                var metadata = new SignatureMetadata(
                    ipAddress: request.SignData.IpAddress,
                    userAgent: request.SignData.UserAgent,
                    deviceInfo: request.SignData.DeviceInfo,
                    location: request.SignData.Location,
                    documentHash: document.ContentHash
                );

                var signedDocumentBytes = await _signatureService.SignDocumentAsync(
                    documentStream,
                    request.SignData.SignatureType,
                    certificateInfo,
                    metadata,
                    cancellationToken);

                using var signedStream = new MemoryStream(signedDocumentBytes);
                var newStoragePath = await _storageService.UploadFileAsync(
                    signedStream,
                    $"{document.FileName}_signed_{DateTime.Now:yyyyMMddHHmmss}{document.FileExtension}",
                    document.MimeType,
                    cancellationToken);

                signedStream.Position = 0;
                var newHash = await _signatureService.ComputeHashAsync(signedStream, cancellationToken);
                var newHashString = Convert.ToHexStringLower(newHash);

                var currentVersion = document.Versions.Max(v => v.VersionNumber);
                document.AddVersion(
                    currentVersion + 1,
                    newStoragePath,
                    newHashString,
                    $"Signed by {signer.Name} on {DateTime.UtcNow}"
                );

                signer.Sign(
                    request.SignData.SignatureType,
                    metadata,
                    certificateInfo,
                    string.Empty // No signature image path provided
                );

                var allSignersInCurrentStep = flow.Signers
                    .Where(s => s.SignOrder == flow.CurrentStep)
                    .ToList();
                
                var allCurrentStepSigned = allSignersInCurrentStep
                    .All(s => s.Status == SignatureStatus.Signed || s.Status == SignatureStatus.Rejected);
                
                if (allCurrentStepSigned)
                {
                    var hasRejection = allSignersInCurrentStep
                        .Any(s => s.Status == SignatureStatus.Rejected);

                    if (hasRejection)
                    {
                        document.UpdateStatus(DocumentStatus.Rejected);
                    }
                    else if (flow.CurrentStep < flow.TotalSteps)
                    {
                        flow.UpdateCurrentStep(flow.CurrentStep + 1);
                        document.UpdateStatus(DocumentStatus.PartiallyCompleted);

                        await NotifyNextSigners(flow,document,cancellationToken);
                    }
                    else
                    {
                        flow.MarkAsCompleted();
                        document.UpdateStatus(DocumentStatus.Completed);

                        await NotifyFlowCompletion(flow,document,cancellationToken);
                    }
                }

                await _unitOfWork.Signers.UpdateAsync(signer, cancellationToken);
                await _unitOfWork.SignatureFlows.UpdateAsync(flow, cancellationToken);
                await _unitOfWork.Documents.UpdateAsync(document, cancellationToken);
            }
            catch (System.Exception)
            {
                
                throw;
            }
        }
    }
}