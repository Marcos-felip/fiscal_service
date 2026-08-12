namespace FiscalService.Domain.Tributacao;

/// <summary>Quadro de COFINS de um item.</summary>
public sealed class CofinsItem
{
    public required SituacaoCofins Situacao { get; init; }

    // ---- por percentual ----
    public decimal? VBC { get; init; }
    public decimal? PCofins { get; init; }

    // ---- por quantidade ----
    public decimal? QBCProd { get; init; }
    public decimal? VAliqProd { get; init; }

    public decimal? VCofins { get; init; }

    public bool TemPercentual => VBC.HasValue || PCofins.HasValue;
    public bool TemQuantidade => QBCProd.HasValue || VAliqProd.HasValue;
}
