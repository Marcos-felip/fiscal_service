using FluentValidation;
using MediatR;

namespace FiscalService.Application.Behaviors;

/// <summary>
/// Roda os <see cref="IValidator{T}"/> registrados antes de qualquer handler MediatR.
///
/// Preferido ao auto-validation do FluentValidation.AspNetCore (que esta obsoleto na v11):
/// a validacao fica na camada Application, vale para qualquer ponto de entrada e nao
/// depende do pipeline de model binding do ASP.NET.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var contexto = new ValidationContext<TRequest>(request);

        var resultados = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(contexto, cancellationToken)));

        var falhas = resultados
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (falhas.Count > 0)
            throw new ValidationException(falhas);

        return await next();
    }
}
