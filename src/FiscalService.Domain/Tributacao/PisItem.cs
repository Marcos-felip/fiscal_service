namespace FiscalService.Domain.Tributacao;

/// <summary>Quadro de PIS de um item.</summary>
public sealed class PisItem
{
    public required SituacaoPis Situacao { get; init; }

    // ---- por percentual ----
    public decimal? VBC { get; init; }
    public decimal? PPis { get; init; }

    // ---- por quantidade ----
    public decimal? QBCProd { get; init; }
    public decimal? VAliqProd { get; init; }

    public decimal? VPis { get; init; }

    public bool TemPercentual => VBC.HasValue || PPis.HasValue;
    public bool TemQuantidade => QBCProd.HasValue || VAliqProd.HasValue;
}
