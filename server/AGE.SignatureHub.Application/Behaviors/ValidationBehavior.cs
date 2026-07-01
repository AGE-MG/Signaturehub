using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AGE.SignatureHub.Application.DTOs.Common;
using FluentValidation;
using MediatR;

namespace AGE.SignatureHub.Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;
        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);
                var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
                var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

                if (failures.Count != 0)
                {
                    if (typeof(BaseResponse).IsAssignableFrom(typeof(TResponse)))
                    {
                        var response = Activator.CreateInstance<TResponse>();
                        if (response is BaseResponse baseResponse)
                        {
                            baseResponse.Success = false;
                            baseResponse.Message = "Validation errors occurred.";
                            baseResponse.Errors = failures.Select(f => f.ErrorMessage).ToList();
                            return response;
                        }
                    }

                    throw new Exceptions.ValidationException(failures);
                }
            }

            return await next();
        }
    }
}
