namespace FiscalService.Application.DTOs;

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
    int Origem,
    string Csosn
);
