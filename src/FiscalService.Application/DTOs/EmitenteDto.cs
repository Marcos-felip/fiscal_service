namespace FiscalService.Application.DTOs;

public record EmitenteDto(
    string Cnpj,
    string RazaoSocial,
    string? NomeFantasia,
    string InscricaoEstadual,
    string Crt,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string CodigoMunicipio,
    string Municipio,
    string Uf,
    string Cep,
    string? Telefone,
    string? Email
);
