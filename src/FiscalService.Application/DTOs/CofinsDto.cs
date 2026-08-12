using System.Text.Json.Serialization;

namespace FiscalService.Application.DTOs;

/// <summary>
/// Quadro de COFINS do item. Mesmas duas formas de apuracao do PIS: por percentual
/// (<c>vBC</c> + <c>pCOFINS</c>) ou por quantidade (<c>qBCProd</c> + <c>vAliqProd</c>).
/// </summary>
/// <param name="Situacao">CST de COFINS (2 digitos).</param>
public record CofinsDto(
    [property: JsonPropertyName("situacao")] string Situacao,

    // ---- por percentual ----
    [property: JsonPropertyName("vBC")] decimal? VBC = null,
    [property: JsonPropertyName("pCOFINS")] decimal? PCofins = null,

    // ---- por quantidade ----
    [property: JsonPropertyName("qBCProd")] decimal? QBCProd = null,
    [property: JsonPropertyName("vAliqProd")] decimal? VAliqProd = null,

    [property: JsonPropertyName("vCOFINS")] decimal? VCofins = null
);
