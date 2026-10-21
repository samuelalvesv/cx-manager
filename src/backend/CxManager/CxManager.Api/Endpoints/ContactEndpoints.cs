using CxManager.Api.Filters;
using CxManager.Application.UseCases.Contacts.Commands.CreateContact;
using CxManager.Application.UseCases.Contacts.Commands.DeleteContact;
using CxManager.Application.UseCases.Contacts.Commands.UpdateContact;
using CxManager.Application.UseCases.Contacts.Queries;
using CxManager.Application.UseCases.Contacts.Queries.GetContactById;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Contacts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace CxManager.Api.Endpoints;

public static class ContactEndpoints
{
    public static RouteGroupBuilder MapContactEndpoints(this RouteGroupBuilder group)
    {
        group.MapGet(
            pattern: "/",
            handler: async Task<Ok<PagedResponse<ContactResponse>>> (
                [AsParameters] GetContactsRequest request,
                IGetContactsUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                PagedResponse<ContactResponse> contacts = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: contacts);
            })
            .WithName("GetContacts")
            .WithSummary("Lista contatos paginados da empresa com busca opcional FTS5.")
            .WithValidation<GetContactsRequest>();

        group.MapGet(
            pattern: "/{id:long}",
            handler: async Task<Ok<ContactResponse>> (
                [AsParameters] GetContactByIdRequest request,
                IGetContactByIdUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                ContactResponse contact = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: contact);
            })
            .WithName("GetContactById")
            .WithSummary("Retorna os detalhes de um contato ativo da empresa.")
            .WithValidation<GetContactByIdRequest>();

        group.MapPost(
            pattern: "/",
            handler: async Task<Created<ContactResponse>> (
                CreateContactRequest request,
                ICreateContactUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                ContactResponse contact = await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.Created(
                    uri: $"/api/contacts/{contact.Id}",
                    value: contact);
            })
            .WithName("CreateContact")
            .WithSummary("Cadastra um novo contato para a empresa ativa.")
            .WithValidation<CreateContactRequest>();

        group.MapPut(
            pattern: "/{id:long}",
            handler: async Task<Ok<ContactResponse>> (
                [AsParameters] GetContactByIdRequest route,
                UpdateContactRequest request,
                IUpdateContactUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                UpdateContactRequest command = request with { Id = route.Id };
                ContactResponse contact = await useCase.ExecuteAsync(
                    request: command,
                    cancellationToken: cancellationToken);

                return TypedResults.Ok(value: contact);
            })
            .WithName("UpdateContact")
            .WithSummary("Atualiza os dados de um contato ativo da empresa.")
            .WithValidation<GetContactByIdRequest>()
            .WithValidation<UpdateContactRequest>();

        group.MapDelete(
            pattern: "/{id:long}",
            handler: async Task<NoContent> (
                [AsParameters] DeleteContactRequest request,
                IDeleteContactUseCase useCase,
                CancellationToken cancellationToken) =>
            {
                await useCase.ExecuteAsync(
                    request: request,
                    cancellationToken: cancellationToken);

                return TypedResults.NoContent();
            })
            .WithName("DeleteContact")
            .WithSummary("Desativa/exclui logicamente um contato da empresa.")
            .WithValidation<DeleteContactRequest>();

        return group;
    }
}
