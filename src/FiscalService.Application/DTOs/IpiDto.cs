using System.Text.Json.Serialization;

namespace FiscalService.Application.DTOs;

/// <summary>
/// Quadro de IPI do item. Opcional: a maioria das NFC-e nao destaca IPI.
/// </summary>
/// <param name="Situacao">CST de IPI (2 digitos).</param>
/// <param name="CEnq">
/// Codigo de enquadramento legal do IPI. Campo exigido pelo layout mesmo quando o item nao
/// e tributado; quando nao informado vale <c>999</c> ("tributacao normal / demais casos").
/// </param>
public record IpiDto(
    [property: JsonPropertyName("situacao")] string Situacao,
    [property: JsonPropertyName("vBC")] decimal? VBC = null,
    [property: JsonPropertyName("pIPI")] decimal? PIpi = null,
    [property: JsonPropertyName("vIPI")] decimal? VIpi = null,
    [property: JsonPropertyName("cEnq")] string? CEnq = null
);
