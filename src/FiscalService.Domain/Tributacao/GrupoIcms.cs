namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Grupo de ICMS do layout da NF-e. Varias situacoes tributarias compartilham o mesmo
/// grupo — CSOSN 102, 103, 300 e 400 caem todas em ICMSSN102, por exemplo — e e o grupo,
/// nao o codigo, que determina quais campos o XML comporta.
/// </summary>
public enum GrupoIcms
{
    Icms00,
    Icms10,
    Icms20,
    Icms30,
    Icms40,
    Icms51,
    Icms60,
    Icms70,
    Icms90,
    IcmsSn101,
    IcmsSn102,
    IcmsSn201,
    IcmsSn202,
    IcmsSn500,
    IcmsSn900
}
