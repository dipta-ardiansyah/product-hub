using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductHub.Application.Common.Exceptions;

namespace ProductHub.Web.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);

            httpContext.Response.ContentType = "application/problem+json";

            if (exception is RequestValidationException validationEx)
            {
                var validationProblem = new ValidationProblemDetails(validationEx.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = "One or more validation errors occurred.",
                    Instance = httpContext.Request.Path
                };
                validationProblem.Extensions["traceId"] = httpContext.TraceIdentifier;

                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(validationProblem, cancellationToken);
                return true;
            }

            var (statusCode, problemDetails) = exception switch
            {
                UnauthorizedException unauthorizedEx => (
                    StatusCodes.Status401Unauthorized,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Title = "Unauthorized",
                        Detail = unauthorizedEx.Message,
                        Instance = httpContext.Request.Path
                    }
                ),
                NotFoundException notFoundEx => (
                    StatusCodes.Status404NotFound,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Not Found",
                        Detail = notFoundEx.Message,
                        Instance = httpContext.Request.Path
                    }
                ),
                ConflictException conflictEx => (
                    StatusCodes.Status409Conflict,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "Conflict",
                        Detail = conflictEx.Message,
                        Instance = httpContext.Request.Path
                    }
                ),
                _ => (
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Internal Server Error",
                        Detail = "An unexpected error occurred. Please try again later.",
                        Instance = httpContext.Request.Path
                    }
                )
            };

            problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            return true;
        }
    }
}