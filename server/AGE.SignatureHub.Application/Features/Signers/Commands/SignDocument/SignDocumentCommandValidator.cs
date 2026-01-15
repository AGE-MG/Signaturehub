using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Domain.Enums;
using FluentValidation;

namespace AGE.SignatureHub.Application.Features.Signers.Commands.SignDocument
{
    public class SignDocumentCommandValidator : AbstractValidator<SignDocumentCommand>
    {
        public SignDocumentCommandValidator()
        {
            RuleFor(x => x.SignData.SignerId)
                .NotEmpty().WithMessage("Signer ID is required");

            RuleFor(x => x.SignData.SignatureType)
                .IsInEnum().WithMessage("Invalid signature type");

            RuleFor(x => x.SignData.IpAddress)
                .NotEmpty().WithMessage("IP Address is required");

            RuleFor(x => x.SignData.CertificateData)
                .NotNull().WithMessage("Certificate data is required")
                .When(x => x.SignData.SignatureType == SignatureType.DigitalA1
                    || x.SignData.SignatureType == SignatureType.DigitalA3);
            
            RuleFor(x => x.SignData.Pin)
                .NotNull().WithMessage("PIN is required")
                .When(x => x.SignData.SignatureType == SignatureType.Eletronic);

        }
    }
}