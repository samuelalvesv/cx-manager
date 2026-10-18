using CxManager.Application.UseCases.Companies.Queries;
using CxManager.Contract.Responses.Companies;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CxManager.Api.Endpoints;

public static class CompanyEndpoints
{
    public static RouteGroupBuilder MapCompanyEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
            pattern: "/",
            handler: async Task<Ok<IReadOnlyList<CompanyResponse>>> (
                IGetCompaniesUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                IReadOnlyList<CompanyResponse> companies = await useCase.ExecuteAsync(cancellationToken: cancellationToken);
                return TypedResults.Ok(value: companies);
            })
            .WithName("GetCompanies")
            .WithSummary("Lista todas as empresas ativas.");

        return group;
    }
}
