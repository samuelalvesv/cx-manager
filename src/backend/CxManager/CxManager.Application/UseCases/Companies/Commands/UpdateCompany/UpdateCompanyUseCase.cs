using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Companies.Commands.UpdateCompany;

public sealed class UpdateCompanyUseCase(ICompanyRepository companyRepository) : IUpdateCompanyUseCase
{
    public async Task<CompanyResponse> ExecuteAsync(
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        Company? company = await companyRepository.GetByIdAsync(
            id: request.Id,
            cancellationToken: cancellationToken);

        if (company is null || company.IsDeleted)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        company.Update(
            name: request.Name,
            description: request.Description);

        await companyRepository.UpdateAsync(
            company: company,
            cancellationToken: cancellationToken);

        return CompanyMapper.ToResponse(company: company);
    }
}
