using FluentValidation;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.TransferDocumentDepartment
{
    public class TransferDocumentDepartmentCommandValidator : AbstractValidator<TransferDocumentDepartmentCommand>
    {
        public TransferDocumentDepartmentCommandValidator()
        {
            RuleFor(x => x.DocumentId)
                .NotEmpty().WithMessage("Document ID is required.");

            RuleFor(x => x.TransferData.TargetUserId)
                .NotEmpty().WithMessage("Target user is required.");

            RuleFor(x => x.TransferData.RequestingUserId)
                .NotEmpty().WithMessage("Requesting user is required.");

            RuleFor(x => x.TransferData.Reason)
                .NotEmpty().WithMessage("Transfer reason is required.")
                .MaximumLength(1000).WithMessage("Transfer reason cannot exceed 1000 characters.");
        }
    }
}
