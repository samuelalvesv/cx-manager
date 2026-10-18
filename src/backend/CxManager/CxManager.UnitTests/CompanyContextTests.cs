using CxManager.Api.Context;
using CxManager.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace CxManager.UnitTests;

public sealed class CompanyContextTests
{
    [Fact]
    public void CompanyId_WhenXCompanyIdHeaderPresent_ShouldResolveSuccessfully()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["X-Company-Id"] = new StringValues(value: "42");

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        HttpCompanyContext context = new(httpContextAccessor: accessor);

        Assert.True(condition: context.HasCompanyId);
        Assert.Equal(expected: 42, actual: context.CompanyId);
        Assert.Equal(expected: 42, actual: context.TryGetCompanyId());
    }

    [Fact]
    public void CompanyId_WhenCompanyIdQueryParamPresent_ShouldResolveSuccessfully()
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.QueryString = new QueryString(value: "?companyId=99");

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        HttpCompanyContext context = new(httpContextAccessor: accessor);

        Assert.True(condition: context.HasCompanyId);
        Assert.Equal(expected: 99, actual: context.CompanyId);
    }

    [Fact]
    public void CompanyId_WhenBothHeaderAndQueryAbsent_ShouldThrowMissingCompanyContextException()
    {
        DefaultHttpContext httpContext = new();
        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        HttpCompanyContext context = new(httpContextAccessor: accessor);

        Assert.False(condition: context.HasCompanyId);
        Assert.Null(@object: context.TryGetCompanyId());

        MissingCompanyContextException ex = Assert.Throws<MissingCompanyContextException>(
            testCode: () => _ = context.CompanyId);

        Assert.Equal(expected: "O identificador da empresa é obrigatório.", actual: ex.Message);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("-1")]
    [InlineData("0")]
    public void CompanyId_WhenHeaderIsInvalid_ShouldThrowMissingCompanyContextException(string invalidHeader)
    {
        DefaultHttpContext httpContext = new();
        httpContext.Request.Headers["X-Company-Id"] = new StringValues(value: invalidHeader);

        HttpContextAccessor accessor = new() { HttpContext = httpContext };
        HttpCompanyContext context = new(httpContextAccessor: accessor);

        Assert.Throws<MissingCompanyContextException>(testCode: () => _ = context.CompanyId);
    }
}
