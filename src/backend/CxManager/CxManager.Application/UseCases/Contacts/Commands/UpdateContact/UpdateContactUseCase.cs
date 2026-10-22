using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses.Contacts;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Contacts.Commands.UpdateContact;

public sealed class UpdateContactUseCase(
    IContactRepository contactRepository,
    IAnalyticsCache analyticsCache,
    ICompanyContext companyContext) : IUpdateContactUseCase
{
    public async Task<ContactResponse> ExecuteAsync(
        UpdateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Contact? contact = await contactRepository.GetByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (contact is null || contact.IsDeleted)
            throw new EntityNotFoundException(message: "Contato não encontrado.");

        bool emailConflict = await contactRepository.ExistsActiveEmailAsync(
            companyId: companyId,
            email: request.Email,
            excludeContactId: request.Id,
            cancellationToken: cancellationToken);

        if (emailConflict)
            throw new EntityConflictException(message: "Já existe outro contato ativo cadastrado com o e-mail informado para esta empresa.");

        contact.Update(
            name: request.Name,
            email: request.Email,
            segment: request.Segment);

        await contactRepository.UpdateAsync(
            contact: contact,
            cancellationToken: cancellationToken);

        analyticsCache.InvalidateCompany(companyId: companyId);

        return ContactMapper.ToResponse(contact: contact);
    }
}
