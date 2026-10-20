using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses.Companies;

namespace CxManager.Application.UseCases.Companies.Commands.CreateCompany;

public interface ICreateCompanyUseCase
{
    Task<CompanyResponse> ExecuteAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default);
}
