using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses.Companies;

namespace CxManager.Application.UseCases.Companies.Commands.UpdateCompany;

public interface IUpdateCompanyUseCase
{
    Task<CompanyResponse> ExecuteAsync(long id, UpdateCompanyRequest request, CancellationToken cancellationToken = default);
}
