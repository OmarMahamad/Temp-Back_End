using BackEnd.Application;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BaseController : ControllerBase
    {
        protected IActionResult HandleResponse<T>(Response<T> response)
        {
            if (response == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Null response returned");

            if (response.IsSuccess)
                return Ok(response);

            // تحويل ErrorCodeResponseType إلى Status Code مناسب
            var statusCode = response.Code switch
            {
                StatusCodeResponeType.Success or
                StatusCodeResponeType.Created or
                StatusCodeResponeType.Updated or
                StatusCodeResponeType.Deleted => StatusCodes.Status200OK,

                StatusCodeResponeType.ValidationError or
                StatusCodeResponeType.BadRequest or
                StatusCodeResponeType.InvalidParameters or
                StatusCodeResponeType.MissingRequiredField => StatusCodes.Status400BadRequest,

                StatusCodeResponeType.Unauthorized or
                StatusCodeResponeType.TokenExpired or
                StatusCodeResponeType.InvalidToken => StatusCodes.Status401Unauthorized,

                StatusCodeResponeType.Forbidden or
                StatusCodeResponeType.AccessDenied => StatusCodes.Status403Forbidden,

                StatusCodeResponeType.NotFound => StatusCodes.Status404NotFound,

                StatusCodeResponeType.AlreadyExists or
                StatusCodeResponeType.DataConflict => StatusCodes.Status409Conflict,

                _ => StatusCodes.Status500InternalServerError
            };

            return StatusCode(statusCode, response);
        }

        // Overload لدعم Response بدون نوع (Response فقط)
        protected IActionResult HandleResponse(Response response)
        {
            if (response == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Null response returned");

            if (response.IsSuccess)
                return Ok(response);

            var statusCode = response.Code switch
            {
                StatusCodeResponeType.ValidationError => StatusCodes.Status400BadRequest,
                StatusCodeResponeType.Unauthorized => StatusCodes.Status401Unauthorized,
                StatusCodeResponeType.Forbidden => StatusCodes.Status403Forbidden,
                StatusCodeResponeType.NotFound => StatusCodes.Status404NotFound,
                StatusCodeResponeType.AlreadyExists => StatusCodes.Status409Conflict,
                _ => StatusCodes.Status500InternalServerError
            };

            return StatusCode(statusCode, response);
        }
    }
}
