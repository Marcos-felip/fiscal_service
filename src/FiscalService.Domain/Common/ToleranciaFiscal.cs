namespace FiscalService.Domain.Common;

/// <summary>
/// Tolerancia unica para comparacao de valores monetarios. Vale tanto para os somatorios
/// da nota (itens x total, pagamentos x total) quanto para a conferencia de base x aliquota
/// do quadro tributario: um centavo de diferenca e arredondamento, nao erro.
/// </summary>
public static class ToleranciaFiscal
{
    public const decimal Centavos = 0.01m;

    /// <summary>Compara dois valores monetarios dentro da tolerancia de um centavo.</summary>
    public static bool Equivalentes(decimal esperado, decimal informado)
        => Math.Abs(esperado - informado) <= Centavos;
}
