using FiscalService.Application.DTOs;
using FiscalService.Domain.Tributacao;

namespace FiscalService.Application.Mappers;

/// <summary>
/// Traduz o bloco tributario do payload para o dominio. Nenhum valor e calculado aqui —
/// so a situacao tributaria vira value object, para que o resto do sistema trabalhe com
/// um codigo ja reconhecido em vez de string solta.
/// </summary>
public static class ImpostoMapper
{
    public static ImpostoItem ToDomain(ImpostoDto dto)
    {
        if (dto is null)
            throw new ArgumentException("Item sem o quadro tributario ('imposto')");

        return new ImpostoItem
        {
            Icms = ToIcms(dto.Icms),
            Pis = ToPis(dto.Pis),
            Cofins = ToCofins(dto.Cofins),
            Ipi = ToIpi(dto.Ipi)
        };
    }

    public static IcmsItem ToIcms(IcmsDto dto) => new()
    {
        Situacao = SituacaoIcms.Criar(dto.Situacao),
        Origem = dto.Origem,
        ModBC = dto.ModBC,
        VBC = dto.VBC,
        PRedBC = dto.PRedBC,
        PIcms = dto.PIcms,
        VIcms = dto.VIcms,
        ModBCST = dto.ModBCST,
        PMvaST = dto.PMvaST,
        PRedBCST = dto.PRedBCST,
        VBCST = dto.VBCST,
        PIcmsST = dto.PIcmsST,
        VIcmsST = dto.VIcmsST,
        VBCSTRet = dto.VBCSTRet,
        VIcmsSTRet = dto.VIcmsSTRet,
        PFcp = dto.PFcp,
        VFcp = dto.VFcp,
        VBCFcpST = dto.VBCFcpST,
        PFcpST = dto.PFcpST,
        VFcpST = dto.VFcpST,
        PCredSn = dto.PCredSn,
        VCredIcmsSn = dto.VCredIcmsSn
    };

    public static PisItem ToPis(PisDto dto) => new()
    {
        Situacao = SituacaoPis.Criar(dto.Situacao),
        VBC = dto.VBC,
        PPis = dto.PPis,
        QBCProd = dto.QBCProd,
        VAliqProd = dto.VAliqProd,
        VPis = dto.VPis
    };

    public static CofinsItem ToCofins(CofinsDto dto) => new()
    {
        Situacao = SituacaoCofins.Criar(dto.Situacao),
        VBC = dto.VBC,
        PCofins = dto.PCofins,
        QBCProd = dto.QBCProd,
        VAliqProd = dto.VAliqProd,
        VCofins = dto.VCofins
    };

    public static IpiItem? ToIpi(IpiDto? dto)
    {
        if (dto is null)
            return null;

        return new IpiItem
        {
            Situacao = SituacaoIpi.Criar(dto.Situacao),
            VBC = dto.VBC,
            PIpi = dto.PIpi,
            VIpi = dto.VIpi,
            CEnq = string.IsNullOrWhiteSpace(dto.CEnq) ? IpiItem.EnquadramentoPadrao : dto.CEnq.Trim()
        };
    }
}
