using FiscalService.Domain.Common;
using FiscalService.Domain.Tributacao;

namespace FiscalService.Domain.Entities;

/// <summary>
/// Item de um documento fiscal. Serve a NFC-e e a NF-e sem diferenca: depois que o
/// quadro tributario passou a vir pronto do backend, nao sobrou nada de especifico
/// de modelo aqui.
/// </summary>
public class ItemFiscal : BaseEntity
{
    public int NumeroItem { get; private set; }
    public string CodigoProduto { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Ncm { get; private set; } = string.Empty;
    public string? Cest { get; init; }
    public string Cfop { get; private set; } = string.Empty;
    public string UnidadeComercial { get; private set; } = string.Empty;
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string? Gtin { get; init; }

    /// <summary>
    /// Quadro tributario decidido e calculado pelo backend. E ele que manda na montagem dos
    /// grupos de imposto — a situacao tributaria e a origem da mercadoria vem de dentro dele.
    /// </summary>
    public ImpostoItem Imposto { get; private set; }

    public ItemFiscal(int numeroItem, string codigoProduto, string descricao, string ncm, string cfop,
        string unidadeComercial, decimal quantidade, decimal valorUnitario, ImpostoItem imposto)
    {
        NumeroItem = numeroItem;
        CodigoProduto = codigoProduto;
        Descricao = descricao;
        Ncm = ncm;
        Cfop = cfop;
        UnidadeComercial = unidadeComercial;
        Quantidade = quantidade;
        ValorUnitario = valorUnitario;
        ValorTotal = quantidade * valorUnitario;
        Imposto = imposto ?? throw new ArgumentNullException(nameof(imposto));
    }
}
