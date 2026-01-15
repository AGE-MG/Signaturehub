using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.Commands.CreateSignatureFlow
{
    public class CreateSignatureFlowCommandValidator : AbstractValidator<CreateSignatureFlowCommand>
    {
        public CreateSignatureFlowCommandValidator()
        {
            RuleFor(x => x.FlowData.DocumentId)
                .NotEmpty().WithMessage("Document ID is required");
            
            RuleFor(x => x.FlowData.FlowName)
                .NotEmpty().WithMessage("Flow name is required")
                .MaximumLength(100).WithMessage("Flow name must not exceed 100 characters");

            RuleFor(x => x.FlowData.FlowType)
                .IsInEnum().WithMessage("Invalid flow type");

            RuleFor(x => x.FlowData.Signers)
                .NotEmpty().WithMessage("At least one signer is required")
                .Must(signers => signers.Count > 0).WithMessage("At least one signer is required");

            RuleForEach(x => x.FlowData.Signers).ChildRules(signers =>
            {
                signers.RuleFor(s => s.Name)
                    .NotEmpty().WithMessage("Signer name is required")
                    .MaximumLength(200).WithMessage("Signer name must not exceed 200 characters");

                signers.RuleFor(s => s.Email)
                    .NotEmpty().WithMessage("Signer email is required")
                    .EmailAddress().WithMessage("Invalid email format");

                signers.RuleFor(s => s.Document)
                    .NotEmpty().WithMessage("Signer document(CPF/CNPJ) is required");

                signers.RuleFor(s => s.Role)
                    .IsInEnum().WithMessage("Invalid signer role");

                signers.RuleFor(s => s.SignOrder)
                    .GreaterThan(0).WithMessage("Signing order must be greater than zero");
            });
        }
    }
}