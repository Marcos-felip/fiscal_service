using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FluentValidation;

namespace FiscalService.Application.Validators;

public class PagamentoDtoValidator : AbstractValidator<PagamentoDto>
{
    public PagamentoDtoValidator()
    {
        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("Tipo de pagamento e obrigatorio")
            .Must(tipo => PagamentoMapper.TryToTipoPagamento(tipo, out _))
            .WithMessage($"Tipo de pagamento invalido. Valores aceitos: {string.Join(", ", PagamentoMapper.TiposAceitos)}");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor do pagamento deve ser maior que zero");
    }
}
