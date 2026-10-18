using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CxManager.Application.Interfaces;
using CxManager.Domain.Exceptions;
using Microsoft.Extensions.Primitives;

namespace CxManager.Api.Context;

public sealed class HttpCompanyContext(IHttpContextAccessor httpContextAccessor) : ICompanyContext
{
    private long? _cachedCompanyId;
    private bool _resolved;

    public long CompanyId
    {
        [SuppressMessage(category: "Design", checkId: "CA1065", Justification = "Acesso ao contexto de empresa obrigatório lança MissingCompanyContextException para fail-fast quando ausente.")]
        get
        {
            EnsureResolved();
            if (!_cachedCompanyId.HasValue)
                throw new MissingCompanyContextException(message: "O identificador da empresa é obrigatório.");

            return _cachedCompanyId.Value;
        }
    }

    public bool HasCompanyId
    {
        get
        {
            EnsureResolved();
            return _cachedCompanyId.HasValue;
        }
    }

    public long? TryGetCompanyId()
    {
        EnsureResolved();
        return _cachedCompanyId;
    }

    public void EnsureCompanyId()
        => _ = CompanyId;

    private void EnsureResolved()
    {
        if (_resolved)
            return;

        _resolved = true;
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null)
            return;

        // 1. Inspeciona cabeçalho X-Company-Id
        if (httpContext.Request.Headers.TryGetValue(key: "X-Company-Id", value: out StringValues headerValue))
        {
            string headerStr = headerValue.ToString();
            if (long.TryParse(s: headerStr, provider: CultureInfo.InvariantCulture, result: out long headerId) && headerId > 0)
            {
                _cachedCompanyId = headerId;
                return;
            }
        }

        // 2. Inspeciona parâmetro de query companyId
        if (!httpContext.Request.Query.TryGetValue(key: "companyId", value: out StringValues queryValue))
            return;

        string queryStr = queryValue.ToString();

        if (long.TryParse(s: queryStr, provider: CultureInfo.InvariantCulture, result: out long queryId) && queryId > 0)
            _cachedCompanyId = queryId;
    }
}
