using AGE.SignatureHub.Application.DTOs.Common;
using AGE.SignatureHub.Application.Exceptions;
using MediatR;

namespace AGE.SignatureHub.Application.Behaviors
{
    public class ExceptionHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                return await next();
            }
            catch (Exception ex) when (typeof(BaseResponse).IsAssignableFrom(typeof(TResponse)))
            {
                var response = Activator.CreateInstance<TResponse>();
                if (response is BaseResponse baseResponse)
                {
                    MapException(baseResponse, ex);
                    return response;
                }

                throw;
            }
        }

        private static void MapException(BaseResponse response, Exception exception)
        {
            response.Success = false;

            switch (exception)
            {
                case ValidationException validationException:
                    response.Message = "VALIDATION_ERROR";
                    response.Errors = validationException.Errors;
                    break;
                case NotFoundException:
                    response.Message = "NOT_FOUND";
                    response.Errors = new List<string> { exception.Message };
                    break;
                case BusinessException:
                    response.Message = "BUSINESS_ERROR";
                    response.Errors = new List<string> { exception.Message };
                    break;
                case UnauthorizedAccessException:
                    response.Message = "UNAUTHORIZED";
                    response.Errors = new List<string> { exception.Message };
                    break;
                default:
                    response.Message = "UNEXPECTED_ERROR";
                    response.Errors = new List<string> { exception.Message };
                    break;
            }
        }
    }
}
