using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Pathfinder.Extensions;

public class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Find the argument of type T
        var validatable = context.Arguments.FirstOrDefault(x => x?.GetType() == typeof(T)) as T;

        if (validatable is not null)
        {
            var validator = context.HttpContext.RequestServices.GetService(typeof(IValidator<T>)) as IValidator<T>;

            if (validator is not null)
            {
                var validationResult = await validator.ValidateAsync(validatable);

                if (!validationResult.IsValid)
                {
                    return Results.ValidationProblem(validationResult.ToDictionary());
                }
            }
        }

        return await next(context);
    }
}
