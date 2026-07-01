using AGE.SignatureHub.Application.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace AGE.SignatureHub.API.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult HandleResponse(BaseResponse response)
        {
            if (response.Success)
            {
                return Ok(response);
            }

            return HandleFailure(response);
        }

        protected IActionResult HandleCreatedResponse<T>(BaseResponse<T> response, string actionName, object routeValues)
        {
            if (response.Success)
            {
                return CreatedAtAction(actionName, routeValues, response);
            }

            return HandleFailure(response);
        }

        protected IActionResult HandleNoContentResponse(BaseResponse response)
        {
            if (response.Success)
            {
                return NoContent();
            }

            return HandleFailure(response);
        }

        private IActionResult HandleFailure(BaseResponse response)
        {
            return response.Message switch
            {
                "NOT_FOUND" => NotFound(response),
                "UNAUTHORIZED" => Unauthorized(response),
                "FORBIDDEN" => StatusCode(StatusCodes.Status403Forbidden, response),
                _ => BadRequest(response)
            };
        }
    }
}
