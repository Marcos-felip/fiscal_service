namespace FiscalService.Domain.Tributacao;

/// <summary>Quadro de IPI de um item.</summary>
public sealed class IpiItem
{
    /// <summary>
    /// Enquadramento legal usado quando o payload nao informa nenhum. 999 e o codigo de
    /// "tributacao normal / demais casos" da tabela da Receita.
    /// </summary>
    public const string EnquadramentoPadrao = "999";

    public required SituacaoIpi Situacao { get; init; }
    public decimal? VBC { get; init; }
    public decimal? PIpi { get; init; }
    public decimal? VIpi { get; init; }

    public string CEnq { get; init; } = EnquadramentoPadrao;
}
