using CxManager.Application.UseCases.Companies.Commands.CreateCompany;
using CxManager.Application.UseCases.Companies.Commands.DeleteCompany;
using CxManager.Application.UseCases.Companies.Commands.UpdateCompany;
using CxManager.Application.UseCases.Companies.Queries;
using CxManager.Application.UseCases.Companies.Queries.GetCompanyById;
using CxManager.Contract.Requests.Companies;
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

        group.MapGet(
            pattern: "/{id:long}",
            handler: async Task<Ok<CompanyResponse>> (
                long id,
                IGetCompanyByIdUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                CompanyResponse company = await useCase.ExecuteAsync(
                    id: id,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: company);
            })
            .WithName("GetCompanyById")
            .WithSummary("Retorna os detalhes de uma empresa ativa.");

        group.MapPost(
            pattern: "/",
            handler: async Task<Created<CompanyResponse>> (
                CreateCompanyRequest request,
                ICreateCompanyUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                CompanyResponse company = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Created(
                    uri: $"/api/companies/{company.Id}",
                    value: company);
            })
            .WithName("CreateCompany")
            .WithSummary("Cadastra uma nova empresa.");

        group.MapPut(
            pattern: "/{id:long}",
            handler: async Task<Ok<CompanyResponse>> (
                long id,
                UpdateCompanyRequest request,
                IUpdateCompanyUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                CompanyResponse company = await useCase.ExecuteAsync(
                    id: id,
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: company);
            })
            .WithName("UpdateCompany")
            .WithSummary("Atualiza uma empresa ativa.");

        group.MapDelete(
            pattern: "/{id:long}",
            handler: async Task<NoContent> (
                long id,
                IDeleteCompanyUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                await useCase.ExecuteAsync(
                    id: id,
                    cancellationToken: cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName("DeleteCompany")
            .WithSummary("Desativa/exclui logicamente uma empresa.");

        return group;
    }
}
