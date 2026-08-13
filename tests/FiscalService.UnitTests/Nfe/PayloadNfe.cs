using FiscalService.Application.DTOs;
using FiscalService.Application.UseCases.EmitirNfe;
using FiscalService.Domain.Enums;
using FiscalService.UnitTests.Tributacao;

namespace FiscalService.UnitTests.Nfe;

/// <summary>
/// Monta o payload de NF-e valido do recorte atual — venda interna, saida, finalidade normal,
/// destinatario pessoa juridica contribuinte em MG. Cada teste altera o unico campo que quer
/// exercitar, para que a recusa observada so possa vir dele.
/// </summary>
internal static class PayloadNfe
{
    internal const string CnpjEmitente = "51720322000146";
    internal const string CnpjDestinatario = "11223344000186";
    internal const string CertificadoFalso = "QUJD";

    internal static EmitenteDto Emitente(string uf = "MG") => new(
        Cnpj: CnpjEmitente,
        RazaoSocial: "Sal e Fogo Braga LTDA",
        NomeFantasia: "Sal e Fogo",
        InscricaoEstadual: "0046845300054",
        Crt: "1",
        Logradouro: "Rua Joviniano Ramos",
        Numero: "446",
        Complemento: null,
        Bairro: "Sao Jose",
        CodigoMunicipio: "3143302",
        Municipio: "Montes Claros",
        Uf: uf,
        Cep: "39400347",
        Telefone: "3898842804",
        Email: null);

    internal static DestinatarioNfeDto Destinatario(
        string? cpfCnpj = null,
        string uf = "MG",
        int indicadorIe = (int)IndicadorIeDestinatario.Contribuinte,
        string? inscricaoEstadual = "0011223340012") => new(
        CpfCnpj: cpfCnpj ?? CnpjDestinatario,
        Nome: "Construtora Norte Mineira LTDA",
        Logradouro: "Avenida Ovidio de Abreu",
        Numero: "1200",
        Complemento: null,
        Bairro: "Centro",
        CodigoMunicipio: "3143302",
        Municipio: "Montes Claros",
        Uf: uf,
        Cep: "39400001",
        IndicadorIe: indicadorIe,
        InscricaoEstadual: inscricaoEstadual,
        Telefone: "3832211000",
        Email: null);

    internal static ItemNfceDto Item(int numero = 1, string cfop = "5102", decimal valorUnitario = 100m) => new(
        NumeroItem: numero,
        CodigoProduto: $"PROD-{numero}",
        Descricao: $"Produto {numero}",
        Ncm: "22021000",
        Cest: null,
        Cfop: cfop,
        UnidadeComercial: "UN",
        Quantidade: 1m,
        ValorUnitario: valorUnitario,
        Gtin: null,
        Imposto: ImpostoDto());

    /// <summary>CSOSN 102 com PIS/COFINS nao tributados — o quadro que a etapa 1 fixou.</summary>
    internal static ImpostoDto ImpostoDto() => new(
        Icms: new IcmsDto("102", 0),
        Pis: new PisDto("07"),
        Cofins: new CofinsDto("07"));

    internal static EmitirNfeRequest Valido() => new()
    {
        Emitente = Emitente(),
        Destinatario = Destinatario(),
        Itens = new List<ItemNfceDto> { Item() },
        Pagamentos = new List<PagamentoDto> { new("dinheiro", 100m) },
        ValorTotal = 100m,
        CertificadoBase64 = CertificadoFalso,
        CertificadoSenha = "senha",
        Serie = 1,
        Numero = 1,
        Ambiente = "homologacao",
        TipoOperacao = (int)TipoOperacao.Saida,
        Finalidade = (int)FinalidadeNfe.Normal,
        ConsumidorFinal = false,
        Presenca = (int)PresencaComprador.Presencial
    };
}
