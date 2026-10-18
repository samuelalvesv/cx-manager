using CxManager.Application.Mappers;
using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Companies.Queries;

public sealed class GetCompaniesUseCase(ICompanyRepository companyRepository) : IGetCompaniesUseCase
{
    public async Task<IReadOnlyList<CompanyResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Company> companies = await companyRepository.GetAllActiveAsync(cancellationToken: cancellationToken);
        return CompanyMapper.ToResponseList(companies: companies);
    }
}
