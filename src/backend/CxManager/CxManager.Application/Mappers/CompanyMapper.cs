using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace CxManager.Application.Mappers;

[Mapper]
public static partial class CompanyMapper
{
    [MapperIgnoreSource(nameof(Company.DeletedAt))]
    [MapperIgnoreSource(nameof(Company.IsDeleted))]
    public static partial CompanyResponse ToResponse(Company company);

    public static partial IReadOnlyList<CompanyResponse> ToResponseList(IReadOnlyList<Company> companies);
}
