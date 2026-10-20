using CxManager.Contract.Responses.Companies;

namespace CxManager.Application.UseCases.Companies.Queries.GetCompanyById;

public interface IGetCompanyByIdUseCase
{
    Task<CompanyResponse> ExecuteAsync(long id, CancellationToken cancellationToken = default);
}
