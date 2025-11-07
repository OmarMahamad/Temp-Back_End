using BackEnd.Application;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System.Net;

namespace BackEnd.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled Exception");

                StatusCodeResponeType code;
                string message;

                switch (ex)
                {
                    case SqlException sqlEx:
                        code = StatusCodeResponeType.DatabaseError;
                        message = "A database error occurred. Please contact support.";
                        break;

                    case UnauthorizedAccessException:
                        code = StatusCodeResponeType.Unauthorized;
                        message = "Access denied.";
                        break;

                    default:
                        code = StatusCodeResponeType.InternalServerError;
                        message = "An unexpected error occurred.";
                        break;
                }

                var response = Response.Failure(message, code);

                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonConvert.SerializeObject(response));
            }

        }
    }
}
