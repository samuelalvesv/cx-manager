using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses.Contacts;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Contacts.Queries.GetContactById;

public sealed class GetContactByIdUseCase(
    IContactRepository contactRepository,
    ICompanyContext companyContext) : IGetContactByIdUseCase
{
    public async Task<ContactResponse> ExecuteAsync(GetContactByIdRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Contact? contact = await contactRepository.GetByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (contact is null || contact.IsDeleted)
            throw new EntityNotFoundException(message: "Contato não encontrado.");

        return ContactMapper.ToResponse(contact: contact);
    }
}
