using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Contacts;

namespace CxManager.Application.UseCases.Contacts.Queries;

public interface IGetContactsUseCase
{
    Task<PagedResponse<ContactResponse>> ExecuteAsync(
        GetContactsRequest request,
        CancellationToken cancellationToken = default);
}
