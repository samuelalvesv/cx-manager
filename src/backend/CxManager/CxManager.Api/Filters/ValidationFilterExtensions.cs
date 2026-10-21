using CxManager.Contract.Responses;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace CxManager.Api.Filters;

public static class ValidationFilterExtensions
{
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) where TRequest : class
    {
        builder.Produces<ErrorResponse>(statusCode: StatusCodes.Status400BadRequest);

        return builder.AddEndpointFilter(routeHandlerFilter: async (context, next) =>
        {
            IValidator<TRequest> validator = context.HttpContext.RequestServices.GetRequiredService<IValidator<TRequest>>();

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
