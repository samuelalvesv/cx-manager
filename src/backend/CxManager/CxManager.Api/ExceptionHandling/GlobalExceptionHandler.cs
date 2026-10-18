using CxManager.Api.Serialization;
using CxManager.Contract.Responses;
using CxManager.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace CxManager.Api.ExceptionHandling;

public sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        (int statusCode, string errorMessage) = exception switch
        {
            MissingCompanyContextException ex => (StatusCodes.Status400BadRequest, ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            EntityNotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            EntityConflictException ex => (StatusCodes.Status409Conflict, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno no servidor.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
            LogUnhandledException(logger: logger, path: httpContext.Request.Path, exception: exception);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json; charset=utf-8";

        ErrorResponse errorResponse = new() { Error = errorMessage };

        await httpContext.Response.WriteAsJsonAsync(
            value: errorResponse,
            jsonTypeInfo: AppJsonSerializerContext.Default.ErrorResponse,
            cancellationToken: cancellationToken);

        return true;
    }

    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Error,
        Message = "Erro não tratado na requisição HTTP: {Path}")]
    private static partial void LogUnhandledException(ILogger logger, PathString path, Exception exception);
}
