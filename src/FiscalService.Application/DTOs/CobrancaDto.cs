namespace FiscalService.Application.DTOs;

/// <summary>Fatura e duplicatas da NF-e. Presente quando a venda e a prazo.</summary>
public record CobrancaDto(
    string? NumeroFatura,
    decimal? ValorOriginal,
    decimal? ValorDesconto,
    decimal? ValorLiquido,
    List<DuplicataDto>? Duplicatas
);

public record DuplicataDto(
    string Numero,
    DateTime Vencimento,
    decimal Valor
);
