using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FluentValidation;

namespace FiscalService.Application.Validators;

public class EmitenteDtoValidator : AbstractValidator<EmitenteDto>
{
    public EmitenteDtoValidator()
    {
        RuleFor(x => x.Cnpj)
            .NotEmpty().WithMessage("CNPJ do emitente e obrigatorio")
            .Must(ValidacoesFiscais.CnpjValido).WithMessage("CNPJ do emitente e invalido");

        RuleFor(x => x.RazaoSocial)
            .NotEmpty().WithMessage("Razao social do emitente e obrigatoria")
            .MaximumLength(60).WithMessage("Razao social do emitente deve ter no maximo 60 caracteres");

        RuleFor(x => x.NomeFantasia)
            .MaximumLength(60).WithMessage("Nome fantasia do emitente deve ter no maximo 60 caracteres");

        RuleFor(x => x.InscricaoEstadual)
            .NotEmpty().WithMessage("Inscricao estadual do emitente e obrigatoria")
            .MaximumLength(14).WithMessage("Inscricao estadual deve ter no maximo 14 caracteres");

        RuleFor(x => x.Crt)
            .NotEmpty().WithMessage("CRT do emitente e obrigatorio")
            .Must(crt => NfceMapper.TryToCrt(crt, out _))
            .WithMessage("CRT invalido. Use 1 (Simples Nacional), 2 (Simples Nacional - excesso de sublimite), 3 (Regime Normal) ou 4 (Simples Nacional - MEI)");

        // Endereco: o adapter usa todos estes campos para montar o grupo enderEmit.
        RuleFor(x => x.Logradouro)
            .NotEmpty().WithMessage("Logradouro do emitente e obrigatorio")
            .MaximumLength(60).WithMessage("Logradouro do emitente deve ter no maximo 60 caracteres");

        RuleFor(x => x.Numero)
            .NotEmpty().WithMessage("Numero do endereco do emitente e obrigatorio")
            .MaximumLength(60).WithMessage("Numero do endereco deve ter no maximo 60 caracteres");

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Bairro do emitente e obrigatorio")
            .MaximumLength(60).WithMessage("Bairro do emitente deve ter no maximo 60 caracteres");

        RuleFor(x => x.CodigoMunicipio)
            .NotEmpty().WithMessage("Codigo do municipio do emitente e obrigatorio")
            .Must(ValidacoesFiscais.CodigoMunicipioValido)
            .WithMessage("Codigo do municipio deve ser o codigo IBGE com 7 digitos");

        RuleFor(x => x.Municipio)
            .NotEmpty().WithMessage("Municipio do emitente e obrigatorio")
            .MaximumLength(60).WithMessage("Municipio do emitente deve ter no maximo 60 caracteres");

        RuleFor(x => x.Uf)
            .NotEmpty().WithMessage("UF do emitente e obrigatoria")
            .Must(ValidacoesFiscais.UfValida).WithMessage("UF do emitente e invalida");

        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP do emitente e obrigatorio")
            .Must(ValidacoesFiscais.CepValido).WithMessage("CEP do emitente deve ter 8 digitos");

        RuleFor(x => x.Telefone)
            .Must(tel => ValidacoesFiscais.SomenteDigitos(tel).Length is >= 6 and <= 14)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone))
            .WithMessage("Telefone do emitente deve ter entre 6 e 14 digitos");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("E-mail do emitente e invalido")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}
