using FiscalService.Application.DTOs;
using FluentValidation;

namespace FiscalService.Application.Validators;

public class ItemNfceDtoValidator : AbstractValidator<ItemNfceDto>
{
    public ItemNfceDtoValidator()
    {
        RuleFor(x => x.NumeroItem)
            .GreaterThan(0).WithMessage("Numero do item deve ser maior que zero");

        RuleFor(x => x.CodigoProduto)
            .NotEmpty().WithMessage("Codigo do produto e obrigatorio")
            .MaximumLength(60).WithMessage("Codigo do produto deve ter no maximo 60 caracteres");

        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descricao do item e obrigatoria")
            .MaximumLength(120).WithMessage("Descricao do item deve ter no maximo 120 caracteres");

        RuleFor(x => x.Ncm)
            .NotEmpty().WithMessage("NCM do item e obrigatorio")
            .Must(ValidacoesFiscais.NcmValido).WithMessage("NCM deve ter 8 digitos");

        RuleFor(x => x.Cest)
            .Must(cest => ValidacoesFiscais.SomenteDigitos(cest).Length == 7)
            .When(x => !string.IsNullOrWhiteSpace(x.Cest))
            .WithMessage("CEST deve ter 7 digitos");

        RuleFor(x => x.Cfop)
            .NotEmpty().WithMessage("CFOP do item e obrigatorio")
            .Must(ValidacoesFiscais.CfopValido)
            .WithMessage("CFOP deve ter 4 digitos e comecar com 5 (operacao interna, exigida na NFC-e)");

        RuleFor(x => x.UnidadeComercial)
            .NotEmpty().WithMessage("Unidade comercial do item e obrigatoria")
            .MaximumLength(6).WithMessage("Unidade comercial deve ter no maximo 6 caracteres");

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Quantidade deve ser maior que zero");

        RuleFor(x => x.ValorUnitario)
            .GreaterThan(0).WithMessage("Valor unitario deve ser maior que zero");

        RuleFor(x => x.Gtin)
            .Must(ValidacoesFiscais.GtinValido)
            .WithMessage("GTIN invalido: deve ter 8, 12, 13 ou 14 digitos e digito verificador correto");

        RuleFor(x => x.Origem)
            .InclusiveBetween(0, 8).WithMessage("Origem da mercadoria deve estar entre 0 e 8");

        RuleFor(x => x.Csosn)
            .NotEmpty().WithMessage("CSOSN/CST do item e obrigatorio");
    }
}
