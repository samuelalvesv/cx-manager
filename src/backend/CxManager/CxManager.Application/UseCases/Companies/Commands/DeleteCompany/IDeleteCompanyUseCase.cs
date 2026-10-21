using CxManager.Contract.Requests.Companies;

namespace CxManager.Application.UseCases.Companies.Commands.DeleteCompany;

public interface IDeleteCompanyUseCase
{
    Task ExecuteAsync(DeleteCompanyRequest request, CancellationToken cancellationToken = default);
}
