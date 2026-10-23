using System.Globalization;
using System.Text;
using CxManager.Application.Interfaces;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Analytics;
using CxManager.Domain.Export;

namespace CxManager.Application.UseCases.Feedbacks.Queries.ExportFeedbacks;

public sealed class ExportFeedbacksUseCase(
    IFeedbackQueryService feedbackQueryService,
    ICompanyContext companyContext) : IExportFeedbacksUseCase
{
    private const string CsvHeader = "id,survey,contact,score,comment,channel,respondedAt";

    public async Task<byte[]> ExecuteAsync(AnalyticsFilterRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        IReadOnlyList<FeedbackExportModel> rows = await feedbackQueryService.GetForExportAsync(
            companyId: companyId,
            from: request.From,
            to: request.To,
            surveyId: request.SurveyId,
            cancellationToken: cancellationToken);

        StringBuilder sb = new();
        sb.Append(value: CsvHeader);
        sb.Append(value: "\r\n");

        foreach (FeedbackExportModel row in rows)
        {
            string formattedDate = row.RespondedAt.ToUniversalTime().ToString(format: "yyyy-MM-ddTHH:mm:ssZ", formatProvider: CultureInfo.InvariantCulture);

            sb.Append(value: row.Id.ToString(provider: CultureInfo.InvariantCulture));
            sb.Append(value: ',');
            sb.Append(value: Rfc4180Escaper.Escape(value: row.Survey));
            sb.Append(value: ',');
            sb.Append(value: Rfc4180Escaper.Escape(value: row.Contact));
            sb.Append(value: ',');
            sb.Append(value: row.Score.ToString(provider: CultureInfo.InvariantCulture));
            sb.Append(value: ',');
            sb.Append(value: Rfc4180Escaper.Escape(value: row.Comment));
            sb.Append(value: ',');
            sb.Append(value: Rfc4180Escaper.Escape(value: row.Channel));
            sb.Append(value: ',');
            sb.Append(value: Rfc4180Escaper.Escape(value: formattedDate));
            sb.Append(value: "\r\n");
        }

        return Encoding.UTF8.GetBytes(s: sb.ToString());
    }
}
