using CxManager.Contract.Responses.Companies;

namespace CxManager.Application.UseCases.Companies.Queries;

public interface IGetCompaniesUseCase
{
    Task<IReadOnlyList<CompanyResponse>> ExecuteAsync(CancellationToken cancellationToken = default);
}
