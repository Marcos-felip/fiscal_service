using FiscalService.Application.Mappers;
using FiscalService.Application.Validators;
using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;
using FluentValidation;

namespace FiscalService.Application.UseCases.EmitirNfe;

/// <summary>
/// Validador da NF-e modelo 55.
///
/// Alem do que o layout exige, e aqui que o recorte vigente vira <b>recusa nomeada</b>:
/// operacao interestadual, destinatario pessoa fisica, finalidade diferente de normal e nota
/// de entrada sao recusadas dizendo o que aconteceu. O motor nao monta XML aproximando um
/// caso que nao sabe representar — um XML interestadual sem DIFAL e aceito pela SEFAZ e
/// escritura errado, e o contador nao tem como perceber.
/// </summary>
public class EmitirNfeValidator : AbstractValidator<EmitirNfeRequest>
{
    private const decimal ToleranciaCentavos = ToleranciaFiscal.Centavos;

    public EmitirNfeValidator()
    {
        RuleFor(x => x.Emitente)
            .NotNull().WithMessage("Emitente e obrigatorio");

        RuleFor(x => x.Emitente!)
            .SetValidator(new EmitenteDtoValidator())
            .When(x => x.Emitente is not null);

        RuleFor(x => x.Destinatario)
            .NotNull().WithMessage("Destinatario e obrigatorio na NF-e modelo 55");

        RuleFor(x => x.Destinatario!)
            .SetValidator(new DestinatarioNfeDtoValidator())
            .When(x => x.Destinatario is not null);

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("NF-e deve conter ao menos um item");

        RuleForEach(x => x.Itens).SetValidator(new ItemNfceDtoValidator());

        RuleFor(x => x.Pagamentos)
            .NotEmpty().WithMessage("NF-e deve conter ao menos uma forma de pagamento");

        RuleForEach(x => x.Pagamentos).SetValidator(new PagamentoDtoValidator());

        RuleFor(x => x.CertificadoBase64)
            .NotEmpty().WithMessage("Certificado e obrigatorio")
            .Must(SerBase64).WithMessage("Certificado deve estar em base64 valido");

        RuleFor(x => x.CertificadoSenha)
            .NotEmpty().WithMessage("Senha do certificado e obrigatoria");

        RuleFor(x => x.Serie)
            .InclusiveBetween(1, 999).WithMessage("Serie deve estar entre 1 e 999");

        RuleFor(x => x.Numero)
            .InclusiveBetween(1, 999_999_999).WithMessage("Numero deve estar entre 1 e 999999999");

        RuleFor(x => x.Ambiente)
            .NotEmpty().WithMessage("Ambiente e obrigatorio")
            .Must(amb => NfceMapper.TryToAmbiente(amb, out _))
            .WithMessage("Ambiente invalido. Use 'producao' ou 'homologacao'");

        RuleFor(x => x.ValorTotal)
            .GreaterThan(0).WithMessage("Valor total deve ser maior que zero");

        RuleFor(x => x.NaturezaOperacao)
            .MaximumLength(60).WithMessage("Natureza da operacao deve ter no maximo 60 caracteres")
            .When(x => !string.IsNullOrWhiteSpace(x.NaturezaOperacao));

        // ---- o CSC nao pertence a NF-e ----

        RuleFor(x => x.CodigoCsc)
            .Empty().WithMessage("O CSC e exclusivo da NFC-e e nao deve ser enviado na NF-e");

        RuleFor(x => x.IdCsc)
            .Empty().WithMessage("O ID do CSC e exclusivo da NFC-e e nao deve ser enviado na NF-e");

        // ---- o recorte vigente ----

        RuleFor(x => x.TipoOperacao)
            .Must(v => NfeMapper.TryToTipoOperacao(v, out _))
            .WithMessage("Tipo de operacao invalido. Use 0 (entrada) ou 1 (saida)");

        RuleFor(x => x.TipoOperacao)
            .Equal((int)TipoOperacao.Saida)
            .When(x => NfeMapper.TryToTipoOperacao(x.TipoOperacao, out _))
            .WithMessage("Apenas nota de saida e aceita no escopo atual");

        RuleFor(x => x.Finalidade)
            .Must(v => NfeMapper.TryToFinalidade(v, out _))
            .WithMessage("Finalidade invalida. Use 1 (normal), 2 (complementar), 3 (ajuste) ou 4 (devolucao)");

        RuleFor(x => x.Finalidade)
            .Equal((int)FinalidadeNfe.Normal)
            .When(x => NfeMapper.TryToFinalidade(x.Finalidade, out _))
            .WithMessage(x => $"Apenas a finalidade normal e aceita no escopo atual; " +
                              $"recebida: {Descrever((FinalidadeNfe)x.Finalidade)}");

        RuleFor(x => x.Presenca)
            .Must(v => NfeMapper.TryToPresenca(v, out _))
            .WithMessage("Indicador de presenca invalido");

        RuleFor(x => x.Transporte!.Modalidade)
            .Must(v => NfeMapper.TryToModalidadeFrete(v, out _))
            .When(x => x.Transporte is not null)
            .WithMessage("Modalidade de frete invalida. Use 0, 1, 2, 3, 4 ou 9");

        // Operacao interestadual exige CFOP 6xxx, DIFAL e partilha, que estao fora do recorte.
        RuleFor(x => x.Destinatario)
            .Must((request, _) => MesmaUf(request))
            .When(x => x.Emitente is not null && x.Destinatario is not null)
            .WithMessage(x =>
                $"Operacao interestadual esta fora do escopo atual: emitente em {x.Emitente.Uf}, " +
                $"destinatario em {x.Destinatario.Uf}");

        // ---- coerencia de valores ----

        RuleFor(x => x.ValorTotal)
            .Must((request, _) => SomaDosItensBateComTotal(request))
            .When(x => x.Itens.Count > 0 && x.ValorTotal > 0)
            .WithMessage(x =>
                $"Valor total ({FormatoFiscal.Valor(x.ValorTotal)}) diverge da soma dos itens " +
                $"({FormatoFiscal.Valor(SomaItens(x))})");

        RuleFor(x => x.Pagamentos)
            .Must((request, _) => SomaDosPagamentosBateComTotal(request))
            .When(x => x.Itens.Count > 0 && x.Pagamentos.Count > 0)
            .WithMessage(x =>
                $"Soma dos pagamentos ({FormatoFiscal.Valor(SomaPagamentos(x))}) diverge do total " +
                $"da nota ({FormatoFiscal.Valor(SomaItens(x))})");

        RuleFor(x => x.Cobranca!.Duplicatas!)
            .Must(duplicatas => duplicatas.All(d => d.Valor > 0))
            .When(x => x.Cobranca?.Duplicatas is { Count: > 0 })
            .WithMessage("Toda duplicata deve ter valor maior que zero");

        RuleFor(x => x.Cobranca!.Duplicatas!)
            .Must(duplicatas => duplicatas.All(d => !string.IsNullOrWhiteSpace(d.Numero)))
            .When(x => x.Cobranca?.Duplicatas is { Count: > 0 })
            .WithMessage("Toda duplicata deve ter numero");
    }

    private static bool MesmaUf(EmitirNfeRequest x)
        => string.Equals(
            x.Emitente.Uf?.Trim(),
            x.Destinatario.Uf?.Trim(),
            StringComparison.OrdinalIgnoreCase);

    private static string Descrever(FinalidadeNfe finalidade) => finalidade switch
    {
        FinalidadeNfe.Complementar => "complementar",
        FinalidadeNfe.Ajuste => "ajuste",
        FinalidadeNfe.Devolucao => "devolucao",
        _ => "normal"
    };

    private static decimal SomaItens(EmitirNfeRequest x)
        => x.Itens.Sum(i => decimal.Round(i.Quantidade * i.ValorUnitario, 2));

    private static decimal SomaPagamentos(EmitirNfeRequest x)
        => x.Pagamentos.Sum(p => p.Valor);

    private static bool SomaDosItensBateComTotal(EmitirNfeRequest x)
        => Math.Abs(SomaItens(x) - x.ValorTotal) <= ToleranciaCentavos;

    private static bool SomaDosPagamentosBateComTotal(EmitirNfeRequest x)
        => Math.Abs(SomaPagamentos(x) - SomaItens(x)) <= ToleranciaCentavos;

    private static bool SerBase64(string valor)
        => !string.IsNullOrEmpty(valor)
           && Convert.TryFromBase64String(valor, new byte[valor.Length], out _);
}
