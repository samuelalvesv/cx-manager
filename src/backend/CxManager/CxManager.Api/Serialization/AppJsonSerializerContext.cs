using System.Text.Json.Serialization;
using CxManager.Contract.Requests.Analytics;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Requests.Surveys;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Analytics;
using CxManager.Contract.Responses.Companies;
using CxManager.Contract.Responses.Contacts;
using CxManager.Contract.Responses.Feedbacks;
using CxManager.Contract.Responses.Surveys;

namespace CxManager.Api.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CompanyResponse))]
[JsonSerializable(typeof(IReadOnlyList<CompanyResponse>))]
[JsonSerializable(typeof(CompanyImpactSummaryResponse))]
[JsonSerializable(typeof(CompanySurveyImpactItemResponse))]
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
[JsonSerializable(typeof(FeedbackResponse))]
[JsonSerializable(typeof(IReadOnlyList<FeedbackResponse>))]
[JsonSerializable(typeof(CreateFeedbackRequest))]
[JsonSerializable(typeof(UpdateFeedbackRequest))]
[JsonSerializable(typeof(GetFeedbackByIdRequest))]
[JsonSerializable(typeof(GetContactFeedbacksRequest))]
[JsonSerializable(typeof(DeleteFeedbackRequest))]
[JsonSerializable(typeof(AnalyticsFilterRequest))]
[JsonSerializable(typeof(AnalyticsClassResponse))]
[JsonSerializable(typeof(AnalyticsSummaryResponse))]
[JsonSerializable(typeof(MonthlyNpsResponse))]
[JsonSerializable(typeof(IReadOnlyList<MonthlyNpsResponse>))]
[JsonSerializable(typeof(SegmentNpsResponse))]
[JsonSerializable(typeof(IReadOnlyList<SegmentNpsResponse>))]
public sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
