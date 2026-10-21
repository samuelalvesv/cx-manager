using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses.Contacts;

namespace CxManager.Application.UseCases.Contacts.Commands.UpdateContact;

public interface IUpdateContactUseCase
{
    Task<ContactResponse> ExecuteAsync(
        UpdateContactRequest request,
        CancellationToken cancellationToken = default);
}
