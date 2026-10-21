using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses.Contacts;

namespace CxManager.Application.UseCases.Contacts.Queries.GetContactById;

public interface IGetContactByIdUseCase
{
    Task<ContactResponse> ExecuteAsync(GetContactByIdRequest request, CancellationToken cancellationToken = default);
}
