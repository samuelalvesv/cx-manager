using CxManager.Contract.Responses;
using FluentValidation;
using FluentValidation.Results;

namespace CxManager.Api.Filters;

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) where TRequest : class
    {
        return builder.AddEndpointFilter(routeHandlerFilter: async (context, next) =>
        {
            IValidator<TRequest>? validator = context.HttpContext.RequestServices.GetService<IValidator<TRequest>>();
            if (validator is null)
                return await next(context: context);

            for (int i = 0; i < context.Arguments.Count; i++)
            {
                if (context.Arguments[i] is not TRequest request)
                    continue;

                ValidationResult result = await validator.ValidateAsync(
                    instance: request,
                    cancellation: context.HttpContext.RequestAborted);

                if (!result.IsValid)
                    return TypedResults.BadRequest(error: new ErrorResponse
                    {
                        Error = result.Errors[0].ErrorMessage
                    });

                break;
            }

            return await next(context: context);
        });
    }
}
