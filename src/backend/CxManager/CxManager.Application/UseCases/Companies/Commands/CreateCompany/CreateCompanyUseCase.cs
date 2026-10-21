using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Companies.Commands.CreateCompany;

public sealed class CreateCompanyUseCase(ICompanyRepository companyRepository) : ICreateCompanyUseCase
{
    public async Task<CompanyResponse> ExecuteAsync(CreateCompanyRequest request, CancellationToken cancellationToken = default)
    {
        var company = Company.Create(
            name: request.Name,
            description: request.Description);

        long id = await companyRepository.CreateAsync(
            company: company,
            cancellationToken: cancellationToken);

        var createdCompany = Company.Reconstitute(
            id: id,
            name: company.Name,
            description: company.Description,
            createdAt: company.CreatedAt,
            deletedAt: null);

        return CompanyMapper.ToResponse(company: createdCompany);
    }
}
