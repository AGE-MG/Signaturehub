using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace AGE.SignatureHub.Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommandValidator : AbstractValidator<CreateDocumentCommand>
    {
        public CreateDocumentCommandValidator()
        {
            RuleFor(x => x.FileStream)
                .NotNull().WithMessage("File stream is required");

            RuleFor(x => x.FileName)
                .NotEmpty().WithMessage("File name is required.")
                .MaximumLength(255).WithMessage("File name cannot exceed 255 characters");

            RuleFor(x => x.DocumentData.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters");

            RuleFor(x => x.DocumentData.CreatedByUserId)
                .NotEmpty().WithMessage("Creator user ID is required");

            RuleFor(x => x.DocumentData.ExpiresAt)
            .Must(date => !date.HasValue || date.Value > DateTime.UtcNow)
            .When(x => x.DocumentData.ExpiresAt.HasValue)
            .WithMessage("Expiration date must be in the future if provided.");
        }
    }
}