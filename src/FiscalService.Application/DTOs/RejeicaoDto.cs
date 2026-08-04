namespace FiscalService.Application.DTOs;

public record RejeicaoDto(
    string Codigo,
    string Mensagem,
    string? RetornoTecnico
);
