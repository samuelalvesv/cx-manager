using CxManager.Application.Models;
using CxManager.Contract.Responses.Feedbacks;
using Riok.Mapperly.Abstractions;

namespace CxManager.Application.Mappers;

[Mapper]
public static partial class FeedbackMapper
{
    public static partial FeedbackResponse ToResponse(FeedbackReadModel model);

    public static partial IReadOnlyList<FeedbackResponse> ToResponseList(IReadOnlyList<FeedbackReadModel> models);
}
