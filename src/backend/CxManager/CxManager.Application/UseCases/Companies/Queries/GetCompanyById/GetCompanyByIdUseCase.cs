using CxManager.Application.Mappers;
using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Companies.Queries.GetCompanyById;

public sealed class GetCompanyByIdUseCase(ICompanyRepository companyRepository) : IGetCompanyByIdUseCase
{
    public async Task<CompanyResponse> ExecuteAsync(long id, CancellationToken cancellationToken = default)
    {
        Company? company = await companyRepository.GetByIdAsync(
            id: id,
            cancellationToken: cancellationToken);

        if (company is null || company.IsDeleted)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        return CompanyMapper.ToResponse(company: company);
    }
}
