using System.Text.Json.Serialization;

namespace FiscalService.Application.DTOs;

/// <summary>
/// Quadro tributario do item, decidido e calculado pelo backend. O motor apenas traduz
/// para os grupos de imposto do XML.
///
/// <c>ipi</c> e opcional porque a maioria das NFC-e nao destaca IPI; ICMS, PIS e COFINS
/// sao exigidos sempre que o bloco vem.
/// </summary>
public record ImpostoDto(
    [property: JsonPropertyName("icms")] IcmsDto Icms,
    [property: JsonPropertyName("pis")] PisDto Pis,
    [property: JsonPropertyName("cofins")] CofinsDto Cofins,
    [property: JsonPropertyName("ipi")] IpiDto? Ipi = null
);
