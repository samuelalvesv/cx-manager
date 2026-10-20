using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Companies.Commands.DeleteCompany;

public sealed class DeleteCompanyUseCase(ICompanyRepository companyRepository) : IDeleteCompanyUseCase
{
    public async Task ExecuteAsync(long id, CancellationToken cancellationToken = default)
    {
        Company? company = await companyRepository.GetByIdAsync(
            id: id,
            cancellationToken: cancellationToken);

        if (company is null || company.IsDeleted)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        company.SoftDelete();

        await companyRepository.DeleteAsync(
            id: company.Id,
            deletedAt: company.DeletedAt!.Value,
            cancellationToken: cancellationToken);
    }
}
