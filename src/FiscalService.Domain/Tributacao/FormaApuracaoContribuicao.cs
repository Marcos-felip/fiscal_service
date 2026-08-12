namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Como PIS e COFINS se apuram, e portanto qual grupo do XML representa o item.
/// </summary>
public enum FormaApuracaoContribuicao
{
    /// <summary>Nao tributada: o grupo leva so o CST (PISNT/COFINSNT).</summary>
    NaoTributada,

    /// <summary>Base de calculo x aliquota percentual (PISAliq/COFINSAliq).</summary>
    Percentual,

    /// <summary>Quantidade vendida x aliquota por unidade (PISQtde/COFINSQtde).</summary>
    Quantidade,

    /// <summary>
    /// Outras operacoes (PISOutr/COFINSOutr): aceita as duas formas, e quem decide qual
    /// e o par de campos que o payload preencheu.
    /// </summary>
    Outras
}
