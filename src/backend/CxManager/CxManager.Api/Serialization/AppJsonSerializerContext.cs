using System.Text.Json.Serialization;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Companies;
using CxManager.Contract.Responses.Contacts;
using CxManager.Contract.Responses.Surveys;

namespace CxManager.Api.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CompanyResponse))]
[JsonSerializable(typeof(IReadOnlyList<CompanyResponse>))]
[JsonSerializable(typeof(CreateCompanyRequest))]
[JsonSerializable(typeof(UpdateCompanyRequest))]
[JsonSerializable(typeof(GetCompanyByIdRequest))]
[JsonSerializable(typeof(DeleteCompanyRequest))]
[JsonSerializable(typeof(SurveyResponse))]
[JsonSerializable(typeof(IReadOnlyList<SurveyResponse>))]
[JsonSerializable(typeof(CreateSurveyRequest))]
[JsonSerializable(typeof(UpdateSurveyRequest))]
[JsonSerializable(typeof(GetSurveyByIdRequest))]
[JsonSerializable(typeof(DeleteSurveyRequest))]
[JsonSerializable(typeof(ContactResponse))]
[JsonSerializable(typeof(IReadOnlyList<ContactResponse>))]
[JsonSerializable(typeof(PagedResponse<ContactResponse>))]
[JsonSerializable(typeof(CreateContactRequest))]
[JsonSerializable(typeof(UpdateContactRequest))]
[JsonSerializable(typeof(GetContactsRequest))]
[JsonSerializable(typeof(GetContactByIdRequest))]
[JsonSerializable(typeof(DeleteContactRequest))]
public sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
