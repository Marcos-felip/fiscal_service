using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FiscalService.Domain.Tributacao;
using FluentValidation;

namespace FiscalService.Application.Validators;

/// <summary>
/// Confere o quadro tributario do item em duas etapas: primeiro se cada situacao existe,
/// depois se o quadro fecha.
///
/// A ordem importa — sem situacao reconhecida nao ha como saber o que o item deveria trazer,
/// e a segunda etapa devolveria uma lista de campos faltando que so confundiria quem esta
/// lendo o erro.
/// </summary>
public class ImpostoDtoValidator : AbstractValidator<ImpostoDto>
{
    public ImpostoDtoValidator()
    {
        RuleFor(x => x.Icms)
            .NotNull().WithMessage("Quadro tributario do item sem o bloco de ICMS");

        RuleFor(x => x.Pis)
            .NotNull().WithMessage("Quadro tributario do item sem o bloco de PIS");

        RuleFor(x => x.Cofins)
            .NotNull().WithMessage("Quadro tributario do item sem o bloco de COFINS");

        RuleFor(x => x.Icms.Origem)
            .InclusiveBetween(0, 8).WithMessage("Origem da mercadoria deve estar entre 0 e 8")
            .When(x => x.Icms is not null);

        RuleFor(x => x.Icms.Situacao)
            .Must(codigo => SituacaoIcms.TryCriar(codigo, out _))
            .When(x => x.Icms is not null)
            .WithMessage(x => SituacaoIcms.MensagemInvalida(x.Icms.Situacao));

        RuleFor(x => x.Pis.Situacao)
            .Must(codigo => SituacaoPis.TryCriar(codigo, out _))
            .When(x => x.Pis is not null)
            .WithMessage(x => SituacaoPis.MensagemInvalida(x.Pis.Situacao));

        RuleFor(x => x.Cofins.Situacao)
            .Must(codigo => SituacaoCofins.TryCriar(codigo, out _))
            .When(x => x.Cofins is not null)
            .WithMessage(x => SituacaoCofins.MensagemInvalida(x.Cofins.Situacao));

        RuleFor(x => x.Ipi!.Situacao)
            .Must(codigo => SituacaoIpi.TryCriar(codigo, out _))
            .When(x => x.Ipi is not null)
            .WithMessage(x => SituacaoIpi.MensagemInvalida(x.Ipi!.Situacao));

        RuleFor(x => x)
            .Custom((dto, contexto) =>
            {
                if (!SituacoesReconhecidas(dto))
                    return;

                foreach (var erro in ValidacaoQuadroTributario.Validar(ImpostoMapper.ToDomain(dto)))
                    contexto.AddFailure(erro);
            });
    }

    private static bool SituacoesReconhecidas(ImpostoDto dto)
    {
        return dto.Icms is not null
               && dto.Pis is not null
               && dto.Cofins is not null
               && SituacaoIcms.TryCriar(dto.Icms.Situacao, out _)
               && SituacaoPis.TryCriar(dto.Pis.Situacao, out _)
               && SituacaoCofins.TryCriar(dto.Cofins.Situacao, out _)
               && (dto.Ipi is null || SituacaoIpi.TryCriar(dto.Ipi.Situacao, out _));
    }
}
