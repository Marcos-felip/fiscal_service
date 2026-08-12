using System.Text.Json.Serialization;

namespace FiscalService.Application.DTOs;

/// <summary>
/// Quadro de ICMS do item. Quem decide a situacao tributaria e calcula os valores e o
/// backend — aqui os campos so viajam ate o XML.
///
/// Os nomes JSON sao os mesmos das tags do layout da NF-e (<c>vBC</c>, <c>pICMS</c>, ...)
/// e por isso vao explicitos: a politica camelCase do System.Text.Json transformaria
/// <c>VBC</c> em <c>vbc</c>.
///
/// Quase tudo e opcional porque o subconjunto exigido depende da situacao — a regra esta
/// em <c>FiscalService.Domain.Tributacao.SituacaoIcms</c>, nao aqui.
/// </summary>
/// <param name="Situacao">CST (2 digitos, Regime Normal) ou CSOSN (3 digitos, Simples Nacional).</param>
/// <param name="Origem">Origem da mercadoria, de 0 a 8.</param>
public record IcmsDto(
    [property: JsonPropertyName("situacao")] string Situacao,
    [property: JsonPropertyName("origem")] int Origem,

    // ---- ICMS proprio ----
    [property: JsonPropertyName("modBC")] int? ModBC = null,
    [property: JsonPropertyName("vBC")] decimal? VBC = null,
    [property: JsonPropertyName("pRedBC")] decimal? PRedBC = null,
    [property: JsonPropertyName("pICMS")] decimal? PIcms = null,
    [property: JsonPropertyName("vICMS")] decimal? VIcms = null,

    // ---- substituicao tributaria ----
    [property: JsonPropertyName("modBCST")] int? ModBCST = null,
    [property: JsonPropertyName("pMVAST")] decimal? PMvaST = null,
    [property: JsonPropertyName("pRedBCST")] decimal? PRedBCST = null,
    [property: JsonPropertyName("vBCST")] decimal? VBCST = null,
    [property: JsonPropertyName("pICMSST")] decimal? PIcmsST = null,
    [property: JsonPropertyName("vICMSST")] decimal? VIcmsST = null,

    // ---- ST retida anteriormente (CST 60 / CSOSN 500) ----
    [property: JsonPropertyName("vBCSTRet")] decimal? VBCSTRet = null,
    [property: JsonPropertyName("vICMSSTRet")] decimal? VIcmsSTRet = null,

    // ---- fundo de combate a pobreza ----
    [property: JsonPropertyName("pFCP")] decimal? PFcp = null,
    [property: JsonPropertyName("vFCP")] decimal? VFcp = null,
    [property: JsonPropertyName("vBCFCPST")] decimal? VBCFcpST = null,
    [property: JsonPropertyName("pFCPST")] decimal? PFcpST = null,
    [property: JsonPropertyName("vFCPST")] decimal? VFcpST = null,

    // ---- credito do Simples Nacional ----
    [property: JsonPropertyName("pCredSN")] decimal? PCredSn = null,
    [property: JsonPropertyName("vCredICMSSN")] decimal? VCredIcmsSn = null
);
