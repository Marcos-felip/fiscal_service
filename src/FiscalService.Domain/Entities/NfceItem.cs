using FiscalService.Domain.ValueObjects;

namespace FiscalService.Domain.Entities;

public class NfceItem : BaseEntity
{
    public int NumeroItem { get; private set; }
    public string CodigoProduto { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public string Ncm { get; private set; } = string.Empty;
    public string? Cest { get; private set; }
    public string Cfop { get; private set; } = string.Empty;
    public string UnidadeComercial { get; private set; } = string.Empty;
    public decimal Quantidade { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal ValorTotal { get; private set; }
    public string? Gtin { get; private set; }
    public int Origem { get; private set; }
    public string Csosn { get; private set; } = string.Empty;

    public NfceItem(int numeroItem, string codigoProduto, string descricao, string ncm, string cfop,
        string unidadeComercial, decimal quantidade, decimal valorUnitario, int origem, string csosn)
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
        Origem = origem;
        Csosn = csosn;
    }
}
