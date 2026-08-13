namespace FiscalService.Domain.Enums;

/// <summary>
/// Indicador de inscricao estadual do destinatario (<c>indIEDest</c>).
///
/// Nao se deduz do tipo de pessoa: prestadora de servico e pessoa juridica e nao e
/// contribuinte de ICMS. Quem declara e o cadastro do destinatario.
/// </summary>
public enum IndicadorIeDestinatario
{
    /// <summary>Contribuinte de ICMS — exige inscricao estadual.</summary>
    Contribuinte = 1,

    /// <summary>Contribuinte isento de inscricao no cadastro de contribuintes.</summary>
    IsentoDeInscricao = 2,

    /// <summary>Nao contribuinte, com ou sem inscricao em outra atividade.</summary>
    NaoContribuinte = 9
}
