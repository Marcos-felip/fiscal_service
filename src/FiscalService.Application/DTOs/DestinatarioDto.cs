namespace FiscalService.Application.DTOs;

public record DestinatarioDto(
    string? CpfCnpj,
    string? Nome,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? CodigoMunicipio,
    string? Municipio,
    string? Uf,
    string? Cep
);
