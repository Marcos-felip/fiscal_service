using FiscalService.Application.DTOs;
using FluentValidation;

namespace FiscalService.Application.Validators;

/// <summary>
/// Na NFC-e o consumidor e opcional — o bloco todo pode vir vazio. Quando algum campo
/// e informado, ele precisa estar correto.
/// </summary>
public class DestinatarioDtoValidator : AbstractValidator<DestinatarioDto>
{
    public DestinatarioDtoValidator()
    {
        RuleFor(x => x.CpfCnpj)
            .Must(ValidacoesFiscais.CpfOuCnpjValido)
            .When(x => !string.IsNullOrWhiteSpace(x.CpfCnpj))
            .WithMessage("CPF/CNPJ do destinatario e invalido");

        RuleFor(x => x.Nome)
            .MaximumLength(60).WithMessage("Nome do destinatario deve ter no maximo 60 caracteres");

        RuleFor(x => x.Uf)
            .Must(ValidacoesFiscais.UfValida)
            .When(x => !string.IsNullOrWhiteSpace(x.Uf))
            .WithMessage("UF do destinatario e invalida");

        RuleFor(x => x.Cep)
            .Must(ValidacoesFiscais.CepValido)
            .When(x => !string.IsNullOrWhiteSpace(x.Cep))
            .WithMessage("CEP do destinatario deve ter 8 digitos");

        RuleFor(x => x.CodigoMunicipio)
            .Must(ValidacoesFiscais.CodigoMunicipioValido)
            .When(x => !string.IsNullOrWhiteSpace(x.CodigoMunicipio))
            .WithMessage("Codigo do municipio do destinatario deve ser o codigo IBGE com 7 digitos");

        // O mapper so monta o endereco do destinatario quando logradouro e UF vem juntos.
        RuleFor(x => x.Uf)
            .NotEmpty()
            .When(x => !string.IsNullOrWhiteSpace(x.Logradouro))
            .WithMessage("UF do destinatario e obrigatoria quando o logradouro e informado");
    }
}
