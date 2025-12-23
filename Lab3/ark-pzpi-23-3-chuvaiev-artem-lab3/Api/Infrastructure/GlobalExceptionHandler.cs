using Api.ApiResult;
using Api.Infrastructure.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;

namespace Api.Infrastructure;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the exception with full details
        _logger.LogError(exception, 
            "Unhandled exception occurred. Path: {Path}, Method: {Method}", 
            httpContext.Request.Path, 
            httpContext.Request.Method);

        var result = ApiResults.ToProblemDetailsObject(ApplicationErrors.ApplicationError);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = ApiResults.GetStatusCode(ApplicationErrors.ApplicationError.Type);

        await httpContext.Response.WriteAsJsonAsync(result, cancellationToken);

        return true;
    }
}