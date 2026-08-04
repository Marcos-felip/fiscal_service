namespace FiscalService.Domain.Enums;

public enum TipoPagamento
{
    Dinheiro = 1,
    Cheque = 2,
    CartaoCredito = 3,
    CartaoDebito = 4,
    CreditoLoja = 5,
    ValeAlimentacao = 10,
    ValeRefeicao = 11,
    ValePresente = 12,
    ValeCombustivel = 13,
    BoletoBancario = 15,
    Pix = 17,
    SemPagamento = 90,
    Outro = 99
}
