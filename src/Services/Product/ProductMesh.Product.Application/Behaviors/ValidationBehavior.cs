using FluentValidation;
using MediatR;
using ProductMesh.Shared.Wrappers;

namespace ProductMesh.Product.Application.Behaviors;

// MediatR Pipeline Behavior: validates every command/query before it reaches its handler.
// Open/Closed Principle — new validators can be added without modifying this behavior.
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any()) return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        var failures = results.SelectMany(r => r.Errors).Where(f => f is not null).ToList();

        if (failures.Count != 0)
        {
            var errors = failures.Select(f => f.ErrorMessage).ToList();
            // Return Result.Failure if TResponse is a Result type
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(TResponse).GetMethod("Failure", [typeof(List<string>)]);
                if (failureMethod is not null)
                    return (TResponse)failureMethod.Invoke(null, [errors])!;
            }
            throw new ValidationException(failures);
        }

        return await next(cancellationToken);
    }
}
