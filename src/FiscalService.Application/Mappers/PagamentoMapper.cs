using FiscalService.Application.DTOs;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;

namespace FiscalService.Application.Mappers;

public static class PagamentoMapper
{
    public static Pagamento ToDomain(PagamentoDto dto)
    {
        var tipo = ToTipoPagamento(dto.Tipo);
        return new Pagamento(tipo, dto.Valor);
    }

    public static TipoPagamento ToTipoPagamento(string tipo)
    {
        return TryToTipoPagamento(tipo, out var valor)
            ? valor
            : throw new ArgumentException($"Tipo de pagamento invalido: {tipo}");
    }

    /// <summary>Usado tambem pelos validators, para nao duplicar a lista de valores aceitos.</summary>
    public static bool TryToTipoPagamento(string? tipo, out TipoPagamento valor)
    {
        switch (tipo?.Trim().ToLowerInvariant())
        {
            case "dinheiro":
                valor = TipoPagamento.Dinheiro;
                return true;
            case "cheque":
                valor = TipoPagamento.Cheque;
                return true;
            case "cartao_credito":
            case "cartao-credito":
                valor = TipoPagamento.CartaoCredito;
                return true;
            case "cartao_debito":
            case "cartao-debito":
                valor = TipoPagamento.CartaoDebito;
                return true;
            case "credito_loja":
            case "credito-loja":
                valor = TipoPagamento.CreditoLoja;
                return true;
            case "pix":
                valor = TipoPagamento.Pix;
                return true;
            case "boleto":
                valor = TipoPagamento.BoletoBancario;
                return true;
            case "vale_alimentacao":
            case "vale-alimentacao":
                valor = TipoPagamento.ValeAlimentacao;
                return true;
            case "vale_refeicao":
            case "vale-refeicao":
                valor = TipoPagamento.ValeRefeicao;
                return true;
            case "vale_presente":
            case "vale-presente":
                valor = TipoPagamento.ValePresente;
                return true;
            case "vale_combustivel":
            case "vale-combustivel":
                valor = TipoPagamento.ValeCombustivel;
                return true;
            case "sem_pagamento":
            case "sem-pagamento":
                valor = TipoPagamento.SemPagamento;
                return true;
            case "outro":
                valor = TipoPagamento.Outro;
                return true;
            default:
                valor = default;
                return false;
        }
    }

    /// <summary>Valores aceitos em <c>PagamentoDto.Tipo</c>, para mensagens de erro.</summary>
    public static readonly string[] TiposAceitos =
    {
        "dinheiro", "cheque", "cartao_credito", "cartao_debito", "credito_loja", "pix",
        "boleto", "vale_alimentacao", "vale_refeicao", "vale_presente", "vale_combustivel",
        "sem_pagamento", "outro"
    };
}
