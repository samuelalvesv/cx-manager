using CxManager.Contract.Requests.Contacts;

namespace CxManager.Application.UseCases.Contacts.Commands.DeleteContact;

public interface IDeleteContactUseCase
{
    Task ExecuteAsync(DeleteContactRequest request, CancellationToken cancellationToken = default);
}
