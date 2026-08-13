namespace FiscalService.Application.DTOs;

/// <summary>
/// Transporte da NF-e. Quando o grupo inteiro nao vem, o motor declara "sem transporte" —
/// que e o caso de quem retira a mercadoria no balcao.
/// </summary>
public record TransporteDto(
    int Modalidade,
    TransportadoraDto? Transportadora,
    VeiculoDto? Veiculo,
    List<VolumeDto>? Volumes
);

public record TransportadoraDto(
    string CpfCnpj,
    string Nome,
    string? InscricaoEstadual,
    string? Endereco,
    string? Municipio,
    string? Uf
);

public record VeiculoDto(
    string Placa,
    string Uf,
    string? Rntc
);

public record VolumeDto(
    int? Quantidade,
    string? Especie,
    string? Marca,
    string? Numeracao,
    decimal? PesoLiquido,
    decimal? PesoBruto
);
