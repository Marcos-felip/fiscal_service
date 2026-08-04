using FluentValidation;

namespace FiscalService.Application.UseCases.CancelarNfce;

public class CancelarNfceValidator : AbstractValidator<CancelarNfceRequest>
{
    public CancelarNfceValidator()
    {
        RuleFor(x => x.ChaveAcesso).NotEmpty().WithMessage("Chave de acesso e obrigatoria");
        RuleFor(x => x.ProtocoloAutorizacao).NotEmpty().WithMessage("Protocolo de autorizacao e obrigatorio");
        RuleFor(x => x.Justificativa)
            .NotEmpty().WithMessage("Justificativa e obrigatoria")
            .MinimumLength(15).WithMessage("Justificativa deve ter no minimo 15 caracteres");
        RuleFor(x => x.CertificadoBase64).NotEmpty().WithMessage("Certificado e obrigatorio");
        RuleFor(x => x.CertificadoSenha).NotEmpty().WithMessage("Senha do certificado e obrigatoria");
    }
}
