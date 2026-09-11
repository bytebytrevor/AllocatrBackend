using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AllocatrApi.Infrastructure;

public class ApiExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ApiExceptionHandler> _logger;

    public ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException =>
                (
                    StatusCodes.Status400BadRequest,
                    "Invalid request"
                ),

            KeyNotFoundException =>
                (
                    StatusCodes.Status404NotFound,
                    "Resource not found"
                ),

            InvalidOperationException =>
                (
                    StatusCodes.Status409Conflict,
                    "Request conflict"
                ),

            UnauthorizedAccessException =>
                (
                    StatusCodes.Status403Forbidden,
                    "Access denied"
                ),

            _ =>
                (
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred"
                )
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled API exception"
            );
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = statusCode >= 500
                ? "The request could not be completed."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken
        );

        return true;
    }
}