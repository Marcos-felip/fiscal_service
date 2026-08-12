namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Campos do quadro de ICMS, nomeados como as tags do layout da NF-e para que a mensagem
/// de recusa aponte exatamente o que o emitente precisa preencher.
/// </summary>
public enum CampoIcms
{
    ModBC,
    VBC,
    PRedBC,
    PIcms,
    VIcms,
    ModBCST,
    PMvaST,
    PRedBCST,
    VBCST,
    PIcmsST,
    VIcmsST,
    VBCSTRet,
    VIcmsSTRet,
    PFcp,
    VFcp,
    VBCFcpST,
    PFcpST,
    VFcpST,
    PCredSn,
    VCredIcmsSn
}

public static class CampoIcmsExtensions
{
    private static readonly IReadOnlyDictionary<CampoIcms, string> Tags = new Dictionary<CampoIcms, string>
    {
        [CampoIcms.ModBC] = "modBC",
        [CampoIcms.VBC] = "vBC",
        [CampoIcms.PRedBC] = "pRedBC",
        [CampoIcms.PIcms] = "pICMS",
        [CampoIcms.VIcms] = "vICMS",
        [CampoIcms.ModBCST] = "modBCST",
        [CampoIcms.PMvaST] = "pMVAST",
        [CampoIcms.PRedBCST] = "pRedBCST",
        [CampoIcms.VBCST] = "vBCST",
        [CampoIcms.PIcmsST] = "pICMSST",
        [CampoIcms.VIcmsST] = "vICMSST",
        [CampoIcms.VBCSTRet] = "vBCSTRet",
        [CampoIcms.VIcmsSTRet] = "vICMSSTRet",
        [CampoIcms.PFcp] = "pFCP",
        [CampoIcms.VFcp] = "vFCP",
        [CampoIcms.VBCFcpST] = "vBCFCPST",
        [CampoIcms.PFcpST] = "pFCPST",
        [CampoIcms.VFcpST] = "vFCPST",
        [CampoIcms.PCredSn] = "pCredSN",
        [CampoIcms.VCredIcmsSn] = "vCredICMSSN"
    };

    public static string Tag(this CampoIcms campo) => Tags[campo];
}
