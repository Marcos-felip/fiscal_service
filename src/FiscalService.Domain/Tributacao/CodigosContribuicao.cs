namespace FiscalService.Domain.Tributacao;

/// <summary>
/// PIS e COFINS usam a mesma tabela de CST e as mesmas formas de apuracao, entao a tabela
/// mora aqui e <see cref="SituacaoPis"/> e <see cref="SituacaoCofins"/> so a consultam.
/// </summary>
internal static class CodigosContribuicao
{
    private static readonly IReadOnlyDictionary<string, FormaApuracaoContribuicao> Formas = Montar();

    internal static bool TryForma(string codigo, out FormaApuracaoContribuicao forma)
        => Formas.TryGetValue(codigo, out forma);

    internal static IEnumerable<string> Codigos => Formas.Keys;

    internal static string? Normalizar(string? codigo)
    {
        var bruto = codigo?.Trim() ?? string.Empty;

        if (bruto.Length is 0 or > 2 || !bruto.All(char.IsDigit))
            return null;

        return bruto.PadLeft(2, '0');
    }

    private static Dictionary<string, FormaApuracaoContribuicao> Montar()
    {
        var tabela = new Dictionary<string, FormaApuracaoContribuicao>
        {
            // Tributavel pela aliquota basica / diferenciada.
            ["01"] = FormaApuracaoContribuicao.Percentual,
            ["02"] = FormaApuracaoContribuicao.Percentual,

            // Tributavel por quantidade vendida x aliquota por unidade.
            ["03"] = FormaApuracaoContribuicao.Quantidade
        };

        // 04 a 09: monofasica, aliquota zero, isenta, sem incidencia, suspensao — todas
        // sem base de calculo.
        foreach (var codigo in new[] { "04", "05", "06", "07", "08", "09" })
            tabela[codigo] = FormaApuracaoContribuicao.NaoTributada;

        // 49 a 99: outras operacoes de saida, creditos e demais casos. O layout aceita as
        // duas formas de apuracao, e o payload escolhe.
        foreach (var codigo in Outras())
            tabela[codigo] = FormaApuracaoContribuicao.Outras;

        return tabela;
    }

    private static IEnumerable<string> Outras()
    {
        yield return "49";

        for (var codigo = 50; codigo <= 56; codigo++)
            yield return codigo.ToString();

        for (var codigo = 60; codigo <= 67; codigo++)
            yield return codigo.ToString();

        for (var codigo = 70; codigo <= 75; codigo++)
            yield return codigo.ToString();

        yield return "98";
        yield return "99";
    }
}
