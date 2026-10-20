namespace CxManager.Application.UseCases.Companies.Commands.DeleteCompany;

public interface IDeleteCompanyUseCase
{
    Task ExecuteAsync(long id, CancellationToken cancellationToken = default);
}
