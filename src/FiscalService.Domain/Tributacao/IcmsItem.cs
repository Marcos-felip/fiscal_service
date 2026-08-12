namespace FiscalService.Domain.Tributacao;

/// <summary>
/// Quadro de ICMS de um item, como o backend o calculou. Nenhum valor aqui e derivado
/// pelo motor: o que nao veio, nao existe.
/// </summary>
public sealed class IcmsItem
{
    public required SituacaoIcms Situacao { get; init; }
    public required int Origem { get; init; }

    // ---- ICMS proprio ----
    public int? ModBC { get; init; }
    public decimal? VBC { get; init; }
    public decimal? PRedBC { get; init; }
    public decimal? PIcms { get; init; }
    public decimal? VIcms { get; init; }

    // ---- substituicao tributaria ----
    public int? ModBCST { get; init; }
    public decimal? PMvaST { get; init; }
    public decimal? PRedBCST { get; init; }
    public decimal? VBCST { get; init; }
    public decimal? PIcmsST { get; init; }
    public decimal? VIcmsST { get; init; }

    // ---- ST retida anteriormente ----
    public decimal? VBCSTRet { get; init; }
    public decimal? VIcmsSTRet { get; init; }

    // ---- fundo de combate a pobreza ----
    public decimal? PFcp { get; init; }
    public decimal? VFcp { get; init; }
    public decimal? VBCFcpST { get; init; }
    public decimal? PFcpST { get; init; }
    public decimal? VFcpST { get; init; }

    // ---- credito do Simples Nacional ----
    public decimal? PCredSn { get; init; }
    public decimal? VCredIcmsSn { get; init; }

    /// <summary>Le um campo pelo nome da tabela de obrigatorios, para a validacao generica.</summary>
    public decimal? Valor(CampoIcms campo) => campo switch
    {
        CampoIcms.ModBC => ModBC,
        CampoIcms.VBC => VBC,
        CampoIcms.PRedBC => PRedBC,
        CampoIcms.PIcms => PIcms,
        CampoIcms.VIcms => VIcms,
        CampoIcms.ModBCST => ModBCST,
        CampoIcms.PMvaST => PMvaST,
        CampoIcms.PRedBCST => PRedBCST,
        CampoIcms.VBCST => VBCST,
        CampoIcms.PIcmsST => PIcmsST,
        CampoIcms.VIcmsST => VIcmsST,
        CampoIcms.VBCSTRet => VBCSTRet,
        CampoIcms.VIcmsSTRet => VIcmsSTRet,
        CampoIcms.PFcp => PFcp,
        CampoIcms.VFcp => VFcp,
        CampoIcms.VBCFcpST => VBCFcpST,
        CampoIcms.PFcpST => PFcpST,
        CampoIcms.VFcpST => VFcpST,
        CampoIcms.PCredSn => PCredSn,
        CampoIcms.VCredIcmsSn => VCredIcmsSn,
        _ => null
    };
}
