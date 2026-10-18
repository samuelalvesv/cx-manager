using CxManager.Application.Interfaces;

namespace CxManager.Api.Filters;

public sealed class RequireCompanyContextFilter(ICompanyContext companyContext) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        companyContext.EnsureCompanyId();
        return await next(context: context);
    }
}
