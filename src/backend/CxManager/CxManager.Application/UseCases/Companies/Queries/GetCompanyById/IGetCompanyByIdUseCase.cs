using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses.Companies;

namespace CxManager.Application.UseCases.Companies.Queries.GetCompanyById;

public interface IGetCompanyByIdUseCase
{
    Task<CompanyResponse> ExecuteAsync(GetCompanyByIdRequest request, CancellationToken cancellationToken = default);
}
