namespace FiscalService.Application.DTOs;

/// <param name="Imposto">
/// Quadro tributario do item. Obrigatorio: a situacao tributaria e a origem da mercadoria
/// vem de dentro dele.
/// </param>
public record ItemNfceDto(
    int NumeroItem,
    string CodigoProduto,
    string Descricao,
    string Ncm,
    string? Cest,
    string Cfop,
    string UnidadeComercial,
    decimal Quantidade,
    decimal ValorUnitario,
    string? Gtin,
    ImpostoDto Imposto
);
