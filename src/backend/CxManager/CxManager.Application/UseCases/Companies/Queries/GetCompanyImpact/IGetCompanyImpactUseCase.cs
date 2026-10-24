using CxManager.Contract.Responses.Companies;

namespace CxManager.Application.UseCases.Companies.Queries.GetCompanyImpact;

public interface IGetCompanyImpactUseCase
{
    Task<CompanyImpactSummaryResponse> ExecuteAsync(long companyId, CancellationToken cancellationToken = default);
}
