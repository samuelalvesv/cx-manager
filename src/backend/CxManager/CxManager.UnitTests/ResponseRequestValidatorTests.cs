using CxManager.Application.Validators.Feedbacks;
using CxManager.Contract.Requests.Feedbacks;
using FluentValidation.Results;

namespace CxManager.UnitTests;

public sealed class FeedbackRequestValidatorTests
{
    private readonly CreateFeedbackRequestValidator _createValidator = new();
    private readonly UpdateFeedbackRequestValidator _updateValidator = new();
    private readonly GetFeedbackByIdRequestValidator _getByIdValidator = new();
    private readonly GetContactFeedbacksRequestValidator _getContactFeedbacksValidator = new();
    private readonly DeleteFeedbackRequestValidator _deleteValidator = new();

    [Theory]
    [InlineData("whatsapp")]
    [InlineData("email")]
    [InlineData("link")]
    [InlineData("WHATSAPP")]
    [InlineData("Email")]
    public void CreateFeedback_ValidRequest_ShouldBeValid(string channel)
    {
        var request = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = 2,
            Score = 10,
            Channel = channel,
            Comment = "Muito bom!"
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateFeedback_InvalidSurveyId_ShouldBeInvalid(long invalidSurveyId)
    {
        var request = new CreateFeedbackRequest
        {
            SurveyId = invalidSurveyId,
            ContactId = 2,
            Score = 10,
            Channel = "whatsapp",
            Comment = null
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(result.Errors, filter: static e => e.PropertyName == nameof(CreateFeedbackRequest.SurveyId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateFeedback_InvalidContactId_ShouldBeInvalid(long invalidContactId)
    {
        var request = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = invalidContactId,
            Score = 10,
            Channel = "whatsapp",
            Comment = null
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(result.Errors, filter: static e => e.PropertyName == nameof(CreateFeedbackRequest.ContactId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void CreateFeedback_InvalidScoreBounds_ShouldBeInvalid(int invalidScore)
    {
        var request = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = 2,
            Score = invalidScore,
            Channel = "whatsapp",
            Comment = null
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(result.Errors, filter: static e => e.PropertyName == nameof(CreateFeedbackRequest.Score));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("sms")]
    [InlineData("telegram")]
    public void CreateFeedback_InvalidChannel_ShouldBeInvalid(string invalidChannel)
    {
        var request = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = 2,
            Score = 10,
            Channel = invalidChannel,
            Comment = null
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(result.Errors, filter: static e => e.PropertyName == nameof(CreateFeedbackRequest.Channel));
    }

    [Fact]
    public void CreateFeedback_CommentExceeding1000Chars_ShouldBeInvalid()
    {
        var request = new CreateFeedbackRequest
        {
            SurveyId = 1,
            ContactId = 2,
            Score = 10,
            Channel = "whatsapp",
            Comment = new string('A', count: 1001)
        };

        ValidationResult result = _createValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(result.Errors, filter: static e => e.PropertyName == nameof(CreateFeedbackRequest.Comment));
    }

    [Fact]
    public void UpdateFeedback_ValidRequest_ShouldBeValid()
    {
        var request = new UpdateFeedbackRequest
        {
            Id = 1,
            Score = 8,
            Comment = "Comentário revisado"
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    public void UpdateFeedback_InvalidScore_ShouldBeInvalid(int invalidScore)
    {
        var request = new UpdateFeedbackRequest
        {
            Id = 1,
            Score = invalidScore,
            Comment = null
        };

        ValidationResult result = _updateValidator.Validate(instance: request);

        Assert.False(condition: result.IsValid);
        Assert.Contains(result.Errors, filter: static e => e.PropertyName == nameof(UpdateFeedbackRequest.Score));
    }

    [Fact]
    public void GetFeedbackById_ValidId_ShouldBeValid()
    {
        var request = new GetFeedbackByIdRequest { Id = 1 };
        ValidationResult result = _getByIdValidator.Validate(instance: request);
        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetFeedbackById_InvalidId_ShouldBeInvalid(long invalidId)
    {
        var request = new GetFeedbackByIdRequest { Id = invalidId };
        ValidationResult result = _getByIdValidator.Validate(instance: request);
        Assert.False(condition: result.IsValid);
    }

    [Fact]
    public void GetContactFeedbacks_ValidId_ShouldBeValid()
    {
        var request = new GetContactFeedbacksRequest { Id = 1 };
        ValidationResult result = _getContactFeedbacksValidator.Validate(instance: request);
        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetContactFeedbacks_InvalidId_ShouldBeInvalid(long invalidId)
    {
        var request = new GetContactFeedbacksRequest { Id = invalidId };
        ValidationResult result = _getContactFeedbacksValidator.Validate(instance: request);
        Assert.False(condition: result.IsValid);
    }

    [Fact]
    public void DeleteFeedback_ValidId_ShouldBeValid()
    {
        var request = new DeleteFeedbackRequest { Id = 1 };
        ValidationResult result = _deleteValidator.Validate(instance: request);
        Assert.True(condition: result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DeleteFeedback_InvalidId_ShouldBeInvalid(long invalidId)
    {
        var request = new DeleteFeedbackRequest { Id = invalidId };
        ValidationResult result = _deleteValidator.Validate(instance: request);
        Assert.False(condition: result.IsValid);
    }
}
