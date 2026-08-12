namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Quadro tributario completo de um item. Existe para o motor traduzir, nao para decidir:
/// tudo aqui chega pronto do backend.
/// </summary>
public sealed class ImpostoItem
{
    public required IcmsItem Icms { get; init; }
    public required PisItem Pis { get; init; }
    public required CofinsItem Cofins { get; init; }

    /// <summary>Opcional: a maioria das NFC-e nao destaca IPI.</summary>
    public IpiItem? Ipi { get; init; }
}
