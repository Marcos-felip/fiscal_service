using FluentValidation;

namespace FiscalService.Application.UseCases.EmitirNfce;

public class EmitirNfceValidator : AbstractValidator<EmitirNfceRequest>
{
    public EmitirNfceValidator()
    {
        RuleFor(x => x.Emitente).NotNull().WithMessage("Emitente e obrigatorio");
        RuleFor(x => x.Emitente.Cnpj).NotEmpty().WithMessage("CNPJ do emitente e obrigatorio");
        RuleFor(x => x.Emitente.RazaoSocial).NotEmpty().WithMessage("Razao social do emitente e obrigatoria");
        RuleFor(x => x.Emitente.InscricaoEstadual).NotEmpty().WithMessage("Inscricao estadual e obrigatoria");

        RuleFor(x => x.Itens).NotEmpty().WithMessage("NFC-e deve conter ao menos um item");
        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.Ncm).NotEmpty().WithMessage("NCM do item e obrigatorio");
            item.RuleFor(i => i.Cfop).NotEmpty().WithMessage("CFOP do item e obrigatorio");
            item.RuleFor(i => i.Quantidade).GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");
            item.RuleFor(i => i.ValorUnitario).GreaterThan(0).WithMessage("Valor unitario deve ser maior que zero");
        });

        RuleFor(x => x.Pagamentos).NotEmpty().WithMessage("NFC-e deve conter ao menos um pagamento");
        RuleForEach(x => x.Pagamentos).ChildRules(pag =>
        {
            pag.RuleFor(p => p.Valor).GreaterThan(0).WithMessage("Valor do pagamento deve ser maior que zero");
        });

        RuleFor(x => x.CertificadoBase64).NotEmpty().WithMessage("Certificado e obrigatorio");
        RuleFor(x => x.CertificadoSenha).NotEmpty().WithMessage("Senha do certificado e obrigatoria");
        RuleFor(x => x.CodigoCsc).NotEmpty().WithMessage("Codigo CSC e obrigatorio");
        RuleFor(x => x.IdCsc).NotEmpty().WithMessage("Id CSC e obrigatorio");
        RuleFor(x => x.Serie).GreaterThan(0).WithMessage("Serie deve ser maior que zero");
        RuleFor(x => x.Numero).GreaterThan(0).WithMessage("Numero deve ser maior que zero");
        RuleFor(x => x.ValorTotal).GreaterThan(0).WithMessage("Valor total deve ser maior que zero");
    }
}
