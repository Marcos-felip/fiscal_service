using System.Text.Json.Serialization;

namespace FiscalService.Application.DTOs;

/// <summary>
/// Quadro de PIS do item.
///
/// Duas formas de apuracao, e o payload escolhe qual usar pelos campos que preenche:
/// por percentual (<c>vBC</c> + <c>pPIS</c>) ou por quantidade (<c>qBCProd</c> +
/// <c>vAliqProd</c>). Situacao nao tributada nao leva nenhum dos dois.
/// </summary>
/// <param name="Situacao">CST de PIS (2 digitos).</param>
public record PisDto(
    [property: JsonPropertyName("situacao")] string Situacao,

    // ---- por percentual ----
    [property: JsonPropertyName("vBC")] decimal? VBC = null,
    [property: JsonPropertyName("pPIS")] decimal? PPis = null,

    // ---- por quantidade ----
    [property: JsonPropertyName("qBCProd")] decimal? QBCProd = null,
    [property: JsonPropertyName("vAliqProd")] decimal? VAliqProd = null,

    [property: JsonPropertyName("vPIS")] decimal? VPis = null
);
