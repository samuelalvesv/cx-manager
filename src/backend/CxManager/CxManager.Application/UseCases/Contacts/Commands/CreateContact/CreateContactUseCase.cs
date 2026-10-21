using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses.Contacts;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Contacts.Commands.CreateContact;

public sealed class CreateContactUseCase(
    IContactRepository contactRepository,
    ICompanyRepository companyRepository,
    ICompanyContext companyContext) : ICreateContactUseCase
{
    public async Task<ContactResponse> ExecuteAsync(CreateContactRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        bool companyExists = await companyRepository.ExistsActiveAsync(
            id: companyId,
            cancellationToken: cancellationToken);

        if (!companyExists)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        bool emailExists = await contactRepository.ExistsActiveEmailAsync(
            companyId: companyId,
            email: request.Email,
            cancellationToken: cancellationToken);

        if (emailExists)
            throw new EntityConflictException(message: "Já existe um contato ativo cadastrado com o e-mail informado para esta empresa.");

        var contact = Contact.Create(
            companyId: companyId,
            name: request.Name,
            email: request.Email,
            segment: request.Segment);

        long id = await contactRepository.CreateAsync(
            contact: contact,
            cancellationToken: cancellationToken);

        var createdContact = Contact.Reconstitute(
            id: id,
            companyId: contact.CompanyId,
            name: contact.Name,
            email: contact.Email,
            segment: contact.Segment,
            createdAt: contact.CreatedAt,
            deletedAt: null);

        return ContactMapper.ToResponse(contact: createdContact);
    }
}
