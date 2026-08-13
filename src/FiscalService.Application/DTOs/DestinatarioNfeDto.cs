namespace FiscalService.Application.DTOs;

/// <summary>
/// Destinatario da NF-e. Diferente do <see cref="DestinatarioDto"/> da NFC-e, aqui nada e
/// opcional exceto complemento, IE, telefone e e-mail.
///
/// <c>IndicadorIe</c>: 1 contribuinte, 2 isento de inscricao, 9 nao contribuinte. Pessoa
/// juridica nao implica contribuinte — prestadora de servico e PJ e nao e contribuinte de
/// ICMS. Quem declara e o cadastro.
/// </summary>
public record DestinatarioNfeDto(
    string CpfCnpj,
    string Nome,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string CodigoMunicipio,
    string Municipio,
    string Uf,
    string Cep,
    int IndicadorIe,
    string? InscricaoEstadual,
    string? Telefone,
    string? Email
);
