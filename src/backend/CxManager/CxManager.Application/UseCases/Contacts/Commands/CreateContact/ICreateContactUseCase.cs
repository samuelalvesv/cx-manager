using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses.Contacts;

namespace CxManager.Application.UseCases.Contacts.Commands.CreateContact;

public interface ICreateContactUseCase
{
    Task<ContactResponse> ExecuteAsync(CreateContactRequest request, CancellationToken cancellationToken = default);
}
