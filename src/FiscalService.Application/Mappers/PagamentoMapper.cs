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
        return tipo.ToLowerInvariant() switch
        {
            "dinheiro" => TipoPagamento.Dinheiro,
            "cartao_credito" or "cartao-credito" => TipoPagamento.CartaoCredito,
            "cartao_debito" or "cartao-debito" => TipoPagamento.CartaoDebito,
            "pix" => TipoPagamento.Pix,
            "boleto" => TipoPagamento.BoletoBancario,
            "vale_alimentacao" or "vale-alimentacao" => TipoPagamento.ValeAlimentacao,
            "vale_refeicao" or "vale-refeicao" => TipoPagamento.ValeRefeicao,
            "vale_presente" or "vale-presente" => TipoPagamento.ValePresente,
            "outro" => TipoPagamento.Outro,
            "sem_pagamento" or "sem-pagamento" => TipoPagamento.SemPagamento,
            _ => throw new ArgumentException($"Tipo de pagamento invalido: {tipo}")
        };
    }
}
