using FiscalService.Application.Mappers;
using FiscalService.Application.Validators;
using FiscalService.Domain.Eventos;
using FluentValidation;

namespace FiscalService.Application.UseCases.InutilizarNumeracao;

public class InutilizarNumeracaoValidator
    : AbstractValidator<InutilizarNumeracaoRequest>
{
    /// <summary>Modelos que o sistema emite — e, portanto, pode inutilizar.</summary>
    private static readonly int[] ModelosAceitos = { 55, 65 };

    public InutilizarNumeracaoValidator()
    {
        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("CNPJ do emitente e obrigatorio")
            .Must(ValidacoesFiscais.CnpjValido).WithMessage("CNPJ do emitente e invalido");

        RuleFor(x => x.Ano)
            .InclusiveBetween(2000, 2099)
            .WithMessage("Ano da numeracao deve ter 4 digitos");

        RuleFor(x => x.Modelo)
            .Must(m => ModelosAceitos.Contains(m))
            .WithMessage("Modelo deve ser 55 (NF-e) ou 65 (NFC-e)");

        RuleFor(x => x.Serie)
            .InclusiveBetween(RegrasDeEvento.SerieMinima, RegrasDeEvento.SerieMaxima)
            .WithMessage(
                $"Serie deve estar entre {RegrasDeEvento.SerieMinima} e {RegrasDeEvento.SerieMaxima}");

        RuleFor(x => x.NumeroInicial)
            .InclusiveBetween(RegrasDeEvento.NumeroMinimo, RegrasDeEvento.NumeroMaximo)
            .WithMessage("Numero inicial fora da faixa aceita");

        RuleFor(x => x.NumeroFinal)
            .InclusiveBetween(RegrasDeEvento.NumeroMinimo, RegrasDeEvento.NumeroMaximo)
            .WithMessage("Numero final fora da faixa aceita");

        // Igual e valido: inutilizar um numero so e o caso comum.
        RuleFor(x => x.NumeroFinal)
            .GreaterThanOrEqualTo(x => x.NumeroInicial)
            .WithMessage("Numero final deve ser maior ou igual ao inicial");

        RuleFor(x => x.Justificativa)
            .NotEmpty().WithMessage("Justificativa e obrigatoria")
            .MinimumLength(RegrasDeEvento.JustificativaTamanhoMinimo)
            .WithMessage(
                $"Justificativa deve ter no minimo {RegrasDeEvento.JustificativaTamanhoMinimo} caracteres")
            .MaximumLength(RegrasDeEvento.JustificativaTamanhoMaximo)
            .WithMessage(
                $"Justificativa deve ter no maximo {RegrasDeEvento.JustificativaTamanhoMaximo} caracteres");

        RuleFor(x => x.CertificadoBase64)
            .NotEmpty().WithMessage("Certificado e obrigatorio");

        RuleFor(x => x.CertificadoSenha)
            .NotEmpty().WithMessage("Senha do certificado e obrigatoria");

        RuleFor(x => x.Uf)
            .NotEmpty().WithMessage("UF do emitente e obrigatoria")
            .Must(ValidacoesFiscais.UfValida).WithMessage("UF do emitente invalida");

        RuleFor(x => x.Ambiente)
            .NotEmpty().WithMessage("Ambiente e obrigatorio")
            .Must(amb => NfceMapper.TryToAmbiente(amb, out _))
            .WithMessage("Ambiente invalido. Use 'producao' ou 'homologacao'");
    }
}
