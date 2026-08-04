using FiscalService.Application.Mappers;
using FiscalService.Application.Validators;
using FluentValidation;

namespace FiscalService.Application.UseCases.ConsultarNfce;

public class ConsultarNfceValidator : AbstractValidator<ConsultarNfceRequest>
{
    public ConsultarNfceValidator()
    {
        RuleFor(x => x.ChaveAcesso)
            .NotEmpty().WithMessage("Chave de acesso e obrigatoria")
            .Must(ValidacoesFiscais.ChaveAcessoValida)
            .WithMessage("Chave de acesso invalida: deve ter 44 digitos e digito verificador correto");

        RuleFor(x => x.CertificadoBase64)
            .NotEmpty().WithMessage("Certificado e obrigatorio");

        RuleFor(x => x.CertificadoSenha)
            .NotEmpty().WithMessage("Senha do certificado e obrigatoria");

        RuleFor(x => x.Ambiente)
            .NotEmpty().WithMessage("Ambiente e obrigatorio")
            .Must(amb => NfceMapper.TryToAmbiente(amb, out _))
            .WithMessage("Ambiente invalido. Use 'producao' ou 'homologacao'");
    }
}
