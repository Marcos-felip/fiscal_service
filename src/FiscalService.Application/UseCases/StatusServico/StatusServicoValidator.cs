using FiscalService.Application.Mappers;
using FiscalService.Application.Validators;
using FluentValidation;

namespace FiscalService.Application.UseCases.StatusServico;

public class StatusServicoValidator : AbstractValidator<StatusServicoRequest>
{
    public StatusServicoValidator()
    {
        RuleFor(x => x.Ambiente)
            .NotEmpty().WithMessage("Ambiente e obrigatorio")
            .Must(amb => NfceMapper.TryToAmbiente(amb, out _))
            .WithMessage("Ambiente invalido. Use 'producao' ou 'homologacao'");

        RuleFor(x => x.Uf)
            .NotEmpty().WithMessage("UF e obrigatoria")
            .Must(ValidacoesFiscais.UfValida).WithMessage("UF invalida");

        RuleFor(x => x.CertificadoBase64)
            .NotEmpty().WithMessage("Certificado e obrigatorio: a SEFAZ exige certificado no TLS mesmo para consultar status");

        RuleFor(x => x.CertificadoSenha)
            .NotEmpty().WithMessage("Senha do certificado e obrigatoria");
    }
}
