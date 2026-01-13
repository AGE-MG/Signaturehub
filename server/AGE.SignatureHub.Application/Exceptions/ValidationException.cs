using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation.Results;

namespace AGE.SignatureHub.Application.Exceptions
{
    public class ValidationException : Exception
    {
        public List<string> Errors { get; }
        public ValidationException(List<ValidationFailure> failures)
            : base("One or more validation failures have occurred.")
        {
            Errors = failures.Select(f => f.ErrorMessage).ToList();
        }

        public ValidationException(string message)
            : base(message)
        {
            Errors = new List<string> { message };
        }
    }
}