using FiscalService.Application.Mappers;
using FiscalService.Application.Validators;
using FiscalService.Domain.Common;
using FluentValidation;

namespace FiscalService.Application.UseCases.EmitirNfce;

public class EmitirNfceValidator : AbstractValidator<EmitirNfceRequest>
{
    /// <summary>Mesma tolerancia usada na conferencia do quadro tributario (1 centavo).</summary>
    private const decimal ToleranciaCentavos = ToleranciaFiscal.Centavos;

    public EmitirNfceValidator()
    {
        RuleFor(x => x.Emitente)
            .NotNull().WithMessage("Emitente e obrigatorio");

        RuleFor(x => x.Emitente!)
            .SetValidator(new EmitenteDtoValidator())
            .When(x => x.Emitente is not null);

        RuleFor(x => x.Destinatario!)
            .SetValidator(new DestinatarioDtoValidator())
            .When(x => x.Destinatario is not null);

        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("NFC-e deve conter ao menos um item");

        RuleForEach(x => x.Itens).SetValidator(new ItemNfceDtoValidator());

        RuleFor(x => x.Pagamentos)
            .NotEmpty().WithMessage("NFC-e deve conter ao menos uma forma de pagamento");

        RuleForEach(x => x.Pagamentos).SetValidator(new PagamentoDtoValidator());

        RuleFor(x => x.CertificadoBase64)
            .NotEmpty().WithMessage("Certificado e obrigatorio")
            .Must(SerBase64).WithMessage("Certificado deve estar em base64 valido");

        RuleFor(x => x.CertificadoSenha)
            .NotEmpty().WithMessage("Senha do certificado e obrigatoria");

        RuleFor(x => x.CodigoCsc)
            .NotEmpty().WithMessage("Codigo CSC e obrigatorio");

        RuleFor(x => x.IdCsc)
            .NotEmpty().WithMessage("Id CSC e obrigatorio")
            .Must(id => ValidacoesFiscais.SomenteDigitos(id).Length is >= 1 and <= 6)
            .WithMessage("Id CSC deve ter entre 1 e 6 digitos");

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

        // ---- regras cruzadas ----

        // O adapter monta vProd/vNF somando os itens e ignora ValorTotal. Se os dois
        // divergirem, a nota sai com valor diferente do que o backend acredita ter enviado.
        RuleFor(x => x.ValorTotal)
            .Must((request, _) => SomaDosItensBateComTotal(request))
            .When(x => x.Itens.Count > 0 && x.ValorTotal > 0)
            .WithMessage(x =>
                $"Valor total ({FormatoFiscal.Valor(x.ValorTotal)}) diverge da soma dos itens " +
                $"({FormatoFiscal.Valor(SomaItens(x))})");

        // Sem grupo de troco no payload, o somatorio dos pagamentos tem que fechar
        // exatamente com o total da nota — caso contrario a SEFAZ rejeita.
        RuleFor(x => x.Pagamentos)
            .Must((request, _) => SomaDosPagamentosBateComTotal(request))
            .When(x => x.Itens.Count > 0 && x.Pagamentos.Count > 0)
            .WithMessage(x =>
                $"Soma dos pagamentos ({FormatoFiscal.Valor(SomaPagamentos(x))}) diverge do total " +
                $"da nota ({FormatoFiscal.Valor(SomaItens(x))})");
    }

    private static decimal SomaItens(EmitirNfceRequest x)
        => x.Itens.Sum(i => decimal.Round(i.Quantidade * i.ValorUnitario, 2));

    private static decimal SomaPagamentos(EmitirNfceRequest x)
        => x.Pagamentos.Sum(p => p.Valor);

    private static bool SomaDosItensBateComTotal(EmitirNfceRequest x)
        => Math.Abs(SomaItens(x) - x.ValorTotal) <= ToleranciaCentavos;

    private static bool SomaDosPagamentosBateComTotal(EmitirNfceRequest x)
        => Math.Abs(SomaPagamentos(x) - SomaItens(x)) <= ToleranciaCentavos;

    private static bool SerBase64(string valor)
        => !string.IsNullOrEmpty(valor)
           && Convert.TryFromBase64String(valor, new byte[valor.Length], out _);
}
