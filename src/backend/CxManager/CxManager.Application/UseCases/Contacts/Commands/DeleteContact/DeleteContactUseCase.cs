using CxManager.Application.Interfaces;
using CxManager.Contract.Requests.Contacts;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Contacts.Commands.DeleteContact;

public sealed class DeleteContactUseCase(
    IContactRepository contactRepository,
    IAnalyticsCache analyticsCache,
    ICompanyContext companyContext) : IDeleteContactUseCase
{
    public async Task ExecuteAsync(DeleteContactRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Contact? contact = await contactRepository.GetByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (contact is null || contact.IsDeleted)
            throw new EntityNotFoundException(message: "Contato não encontrado.");

        contact.SoftDelete();

        await contactRepository.DeleteAsync(
            id: contact.Id,
            companyId: companyId,
            deletedAt: contact.DeletedAt!.Value,
            cancellationToken: cancellationToken);

        analyticsCache.InvalidateCompany(companyId: companyId);
    }
}
