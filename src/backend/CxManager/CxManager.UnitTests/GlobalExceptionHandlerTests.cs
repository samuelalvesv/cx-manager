using System.Text.Json;
using CxManager.Api.ExceptionHandling;
using CxManager.Contract.Responses;
using CxManager.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace CxManager.UnitTests;

public sealed class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new(logger: NullLogger<GlobalExceptionHandler>.Instance);

    [Fact]
    public async Task TryHandleAsync_WhenMissingCompanyContextException_ShouldReturn400WithErrorResponse()
    {
        DefaultHttpContext context = new();
        MemoryStream responseBodyStream = new();
        context.Response.Body = responseBodyStream;

        MissingCompanyContextException exception = new(message: "O identificador da empresa é obrigatório.");

        bool handled = await _handler.TryHandleAsync(
            httpContext: context,
            exception: exception,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(condition: handled);
        Assert.Equal(expected: StatusCodes.Status400BadRequest, actual: context.Response.StatusCode);

        responseBodyStream.Seek(offset: 0, loc: SeekOrigin.Begin);
        ErrorResponse? errorResponse = await JsonSerializer.DeserializeAsync<ErrorResponse>(
            utf8Json: responseBodyStream,
            options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: errorResponse);
        Assert.Equal(expected: "O identificador da empresa é obrigatório.", actual: errorResponse.Error);
    }

    [Fact]
    public async Task TryHandleAsync_WhenBadHttpRequestException_ShouldReturn400WithErrorResponse()
    {
        DefaultHttpContext context = new();
        MemoryStream responseBodyStream = new();
        context.Response.Body = responseBodyStream;

        BadHttpRequestException exception = new(message: "Parâmetro inválido.");

        bool handled = await _handler.TryHandleAsync(
            httpContext: context,
            exception: exception,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(condition: handled);
        Assert.Equal(expected: StatusCodes.Status400BadRequest, actual: context.Response.StatusCode);

        responseBodyStream.Seek(offset: 0, loc: SeekOrigin.Begin);
        ErrorResponse? errorResponse = await JsonSerializer.DeserializeAsync<ErrorResponse>(
            utf8Json: responseBodyStream,
            options: new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase },
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.NotNull(@object: errorResponse);
        Assert.Equal(expected: "Parâmetro de requisição inválido ou mal formatado.", actual: errorResponse.Error);
    }

    [Fact]
    public async Task TryHandleAsync_WhenEntityNotFoundException_ShouldReturn404()
    {
        DefaultHttpContext context = new();
        MemoryStream responseBodyStream = new();
        context.Response.Body = responseBodyStream;

        EntityNotFoundException exception = new(message: "Empresa não encontrada.");

        bool handled = await _handler.TryHandleAsync(
            httpContext: context,
            exception: exception,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(condition: handled);
        Assert.Equal(expected: StatusCodes.Status404NotFound, actual: context.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_WhenEntityConflictException_ShouldReturn409()
    {
        DefaultHttpContext context = new();
        MemoryStream responseBodyStream = new();
        context.Response.Body = responseBodyStream;

        EntityConflictException exception = new(message: "E-mail já cadastrado.");

        bool handled = await _handler.TryHandleAsync(
            httpContext: context,
            exception: exception,
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(condition: handled);
        Assert.Equal(expected: StatusCodes.Status409Conflict, actual: context.Response.StatusCode);
    }
}
