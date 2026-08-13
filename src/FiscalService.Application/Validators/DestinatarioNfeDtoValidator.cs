using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FiscalService.Domain.Enums;
using FluentValidation;

namespace FiscalService.Application.Validators;

/// <summary>
/// Destinatario da NF-e: nada e opcional aqui, ao contrario da NFC-e.
///
/// As mesmas regras existem como invariante em <c>DestinatarioNfe</c>. A duplicacao e
/// deliberada: a invariante impede o objeto errado de existir, o validator transforma o
/// payload errado em 400 com mensagem em vez de excecao de construcao.
/// </summary>
public class DestinatarioNfeDtoValidator : AbstractValidator<DestinatarioNfeDto>
{
    public DestinatarioNfeDtoValidator()
    {
        RuleFor(x => x.CpfCnpj)
            .NotEmpty().WithMessage("Documento do destinatario e obrigatorio na NF-e")
            .Must(ValidacoesFiscais.CnpjValido)
            .WithMessage("A NF-e exige destinatario pessoa juridica com CNPJ valido; " +
                         "para pessoa fisica, emita NFC-e");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do destinatario e obrigatorio")
            .MaximumLength(60).WithMessage("Nome do destinatario deve ter no maximo 60 caracteres");

        RuleFor(x => x.Logradouro)
            .NotEmpty().WithMessage("Logradouro do destinatario e obrigatorio")
            .MaximumLength(60).WithMessage("Logradouro deve ter no maximo 60 caracteres");

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Numero do endereco do destinatario e obrigatorio");

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Bairro do destinatario e obrigatorio");

        RuleFor(x => x.CodigoMunicipio)
            .NotEmpty().WithMessage("Codigo do municipio do destinatario e obrigatorio")
            .Must(ValidacoesFiscais.CodigoMunicipioValido)
            .WithMessage("Codigo do municipio deve ter 7 digitos (IBGE)");

        RuleFor(x => x.Municipio)
            .NotEmpty().WithMessage("Municipio do destinatario e obrigatorio");

        RuleFor(x => x.Uf)
            .NotEmpty().WithMessage("UF do destinatario e obrigatoria")
            .Must(ValidacoesFiscais.UfValida).WithMessage("UF do destinatario invalida");

        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP do destinatario e obrigatorio")
            .Must(ValidacoesFiscais.CepValido).WithMessage("CEP deve ter 8 digitos");

        RuleFor(x => x.IndicadorIe)
            .Must(v => NfeMapper.TryToIndicadorIe(v, out _))
            .WithMessage("Indicador de IE invalido. Use 1 (contribuinte), 2 (isento) ou 9 (nao contribuinte)");

        // ---- coerencia entre o que foi declarado e o que veio ----

        RuleFor(x => x.InscricaoEstadual)
            .NotEmpty()
            .When(x => x.IndicadorIe == (int)IndicadorIeDestinatario.Contribuinte)
            .WithMessage("Destinatario declarado contribuinte exige inscricao estadual");

        RuleFor(x => x.InscricaoEstadual)
            .Empty()
            .When(x => x.IndicadorIe == (int)IndicadorIeDestinatario.IsentoDeInscricao
                       || x.IndicadorIe == (int)IndicadorIeDestinatario.NaoContribuinte)
            .WithMessage("Destinatario declarado isento ou nao contribuinte nao pode ter inscricao estadual");
    }
}
