using System.Globalization;

namespace FiscalService.Domain.Common;

/// <summary>
/// Formatacao dos valores que aparecem em mensagem de recusa.
///
/// Existe porque o formato padrao segue a cultura do processo: no Windows de
/// desenvolvimento (pt-BR) a mesma recusa sai "10,00" e no contentor Linux, "10.00". A
/// mensagem chega ao lojista pelo backend, entao ela e sempre pt-BR — e, mais importante,
/// e sempre a mesma, independente de onde o motor esteja rodando.
/// </summary>
public static class FormatoFiscal
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    /// <summary>Valor com duas casas, sempre com virgula decimal.</summary>
    public static string Valor(decimal valor) => valor.ToString("F2", PtBr);
}
