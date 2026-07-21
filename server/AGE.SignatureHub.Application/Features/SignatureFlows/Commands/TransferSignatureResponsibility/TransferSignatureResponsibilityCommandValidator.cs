using FluentValidation;

namespace AGE.SignatureHub.Application.Features.SignatureFlows.Commands.TransferSignatureResponsibility
{
    public class TransferSignatureResponsibilityCommandValidator : AbstractValidator<TransferSignatureResponsibilityCommand>
    {
        public TransferSignatureResponsibilityCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty().WithMessage("Document ID is required.");

            RuleFor(x => x.TransferData.RequestingUserId)
                .NotEmpty().WithMessage("Requesting user is required.");

            RuleFor(x => x.TransferData.NewResponsibleName)
                .NotEmpty().WithMessage("New responsible name is required.")
                .MaximumLength(200).WithMessage("New responsible name must not exceed 200 characters.");

            RuleFor(x => x.TransferData.NewResponsibleEmail)
                .NotEmpty().WithMessage("New responsible email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.TransferData.NewResponsibleDocument)
                .NotEmpty().WithMessage("New responsible document (CPF/CNPJ) is required.");
        }
    }
}
