using FiscalService.Domain.Entities;
using FiscalService.Domain.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;

namespace FiscalService.Infrastructure.DFe;

/// <summary>
/// Traduz o quadro tributario do item para os grupos de imposto do XML.
///
/// Tradutor, nao calculadora: nao escolhe situacao tributaria, nao aplica aliquota e nao
/// completa valor que nao veio. Quem decide imposto e o backend — aqui os numeros so trocam
/// de formato. A coerencia do quadro ja foi conferida em
/// <see cref="ValidacaoQuadroTributario"/> antes de chegar aqui.
///
/// Se um grupo do layout ainda nao tem traducao, o caminho e adicionar a situacao a tabela em
/// <see cref="SituacaoIcms"/> e o grupo correspondente aqui — nunca um <c>if</c> de situacao
/// tributaria no adapter.
/// </summary>
public static class TradutorImposto
{
    public static imposto Montar(ItemFiscal item) => Montar(item.Imposto);

    public static imposto Montar(ImpostoItem quadro)
    {
        var tributos = new imposto
        {
            ICMS = new ICMS { TipoICMS = MontarIcms(quadro.Icms) },
            PIS = new PIS { TipoPIS = MontarPis(quadro.Pis) },
            COFINS = new COFINS { TipoCOFINS = MontarCofins(quadro.Cofins) }
        };

        if (quadro.Ipi is not null)
            tributos.IPI = MontarIpi(quadro.Ipi);

        return tributos;
    }

    // ------------------------------------------------------------------------------ ICMS

    public static ICMSBasico MontarIcms(IcmsItem icms)
    {
        var origem = MapearOrigem(icms.Origem);
        var codigo = icms.Situacao.Codigo;

        return icms.Situacao.Grupo switch
        {
            GrupoIcms.Icms00 => new ICMS00
            {
                orig = origem,
                CST = Cst(codigo),
                modBC = ModBC(icms.ModBC),
                vBC = Exigir(icms.VBC, "vBC", codigo),
                pICMS = Exigir(icms.PIcms, "pICMS", codigo),
                vICMS = Exigir(icms.VIcms, "vICMS", codigo),
                pFCP = icms.PFcp,
                vFCP = icms.VFcp
            },

            GrupoIcms.Icms10 => new ICMS10
            {
                orig = origem,
                CST = Cst(codigo),
                modBC = ModBC(icms.ModBC),
                vBC = Exigir(icms.VBC, "vBC", codigo),
                pICMS = Exigir(icms.PIcms, "pICMS", codigo),
                vICMS = Exigir(icms.VIcms, "vICMS", codigo),
                pFCP = icms.PFcp,
                vFCP = icms.VFcp,
                modBCST = ModBCST(icms.ModBCST),
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = Exigir(icms.VBCST, "vBCST", codigo),
                pICMSST = Exigir(icms.PIcmsST, "pICMSST", codigo),
                vICMSST = Exigir(icms.VIcmsST, "vICMSST", codigo),
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST
            },

            GrupoIcms.Icms20 => new ICMS20
            {
                orig = origem,
                CST = Cst(codigo),
                modBC = ModBC(icms.ModBC),
                pRedBC = Exigir(icms.PRedBC, "pRedBC", codigo),
                vBC = Exigir(icms.VBC, "vBC", codigo),
                pICMS = Exigir(icms.PIcms, "pICMS", codigo),
                vICMS = Exigir(icms.VIcms, "vICMS", codigo),
                pFCP = icms.PFcp,
                vFCP = icms.VFcp
            },

            GrupoIcms.Icms30 => new ICMS30
            {
                orig = origem,
                CST = Cst(codigo),
                modBCST = ModBCST(icms.ModBCST),
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = Exigir(icms.VBCST, "vBCST", codigo),
                pICMSST = Exigir(icms.PIcmsST, "pICMSST", codigo),
                vICMSST = Exigir(icms.VIcmsST, "vICMSST", codigo),
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST
            },

            // CST 40, 41 e 50 compartilham o grupo: o XML leva so origem e CST.
            GrupoIcms.Icms40 => new ICMS40
            {
                orig = origem,
                CST = Cst(codigo)
            },

            GrupoIcms.Icms51 => new ICMS51
            {
                orig = origem,
                CST = Cst(codigo),
                modBC = icms.ModBC.HasValue ? ModBC(icms.ModBC) : null,
                pRedBC = icms.PRedBC,
                vBC = icms.VBC,
                pICMS = icms.PIcms,
                vICMS = icms.VIcms,
                pFCP = icms.PFcp,
                vFCP = icms.VFcp
            },

            GrupoIcms.Icms60 => new ICMS60
            {
                orig = origem,
                CST = Cst(codigo),
                vBCSTRet = icms.VBCSTRet,
                vICMSSTRet = icms.VIcmsSTRet
            },

            GrupoIcms.Icms70 => new ICMS70
            {
                orig = origem,
                CST = Cst(codigo),
                modBC = ModBC(icms.ModBC),
                pRedBC = Exigir(icms.PRedBC, "pRedBC", codigo),
                vBC = Exigir(icms.VBC, "vBC", codigo),
                pICMS = Exigir(icms.PIcms, "pICMS", codigo),
                vICMS = Exigir(icms.VIcms, "vICMS", codigo),
                pFCP = icms.PFcp,
                vFCP = icms.VFcp,
                modBCST = ModBCST(icms.ModBCST),
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = Exigir(icms.VBCST, "vBCST", codigo),
                pICMSST = Exigir(icms.PIcmsST, "pICMSST", codigo),
                vICMSST = Exigir(icms.VIcmsST, "vICMSST", codigo),
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST
            },

            GrupoIcms.Icms90 => new ICMS90
            {
                orig = origem,
                CST = Cst(codigo),
                modBC = icms.ModBC.HasValue ? ModBC(icms.ModBC) : null,
                vBC = icms.VBC,
                pRedBC = icms.PRedBC,
                pICMS = icms.PIcms,
                vICMS = icms.VIcms,
                pFCP = icms.PFcp,
                vFCP = icms.VFcp,
                modBCST = icms.ModBCST.HasValue ? ModBCST(icms.ModBCST) : null,
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = icms.VBCST,
                pICMSST = icms.PIcmsST,
                vICMSST = icms.VIcmsST,
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST
            },

            GrupoIcms.IcmsSn101 => new ICMSSN101
            {
                orig = origem,
                CSOSN = Csosn(codigo),
                pCredSN = Exigir(icms.PCredSn, "pCredSN", codigo),
                vCredICMSSN = Exigir(icms.VCredIcmsSn, "vCredICMSSN", codigo)
            },

            // CSOSN 102, 103, 300 e 400 compartilham o grupo: so origem e CSOSN.
            GrupoIcms.IcmsSn102 => new ICMSSN102
            {
                orig = origem,
                CSOSN = Csosn(codigo)
            },

            GrupoIcms.IcmsSn201 => new ICMSSN201
            {
                orig = origem,
                CSOSN = Csosn(codigo),
                modBCST = ModBCST(icms.ModBCST),
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = Exigir(icms.VBCST, "vBCST", codigo),
                pICMSST = Exigir(icms.PIcmsST, "pICMSST", codigo),
                vICMSST = Exigir(icms.VIcmsST, "vICMSST", codigo),
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST,
                pCredSN = Exigir(icms.PCredSn, "pCredSN", codigo),
                vCredICMSSN = Exigir(icms.VCredIcmsSn, "vCredICMSSN", codigo)
            },

            // CSOSN 202 e 203 compartilham o grupo.
            GrupoIcms.IcmsSn202 => new ICMSSN202
            {
                orig = origem,
                CSOSN = Csosn(codigo),
                modBCST = ModBCST(icms.ModBCST),
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = Exigir(icms.VBCST, "vBCST", codigo),
                pICMSST = Exigir(icms.PIcmsST, "pICMSST", codigo),
                vICMSST = Exigir(icms.VIcmsST, "vICMSST", codigo),
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST
            },

            GrupoIcms.IcmsSn500 => new ICMSSN500
            {
                orig = origem,
                CSOSN = Csosn(codigo),
                vBCSTRet = icms.VBCSTRet,
                vICMSSTRet = icms.VIcmsSTRet
            },

            GrupoIcms.IcmsSn900 => new ICMSSN900
            {
                orig = origem,
                CSOSN = Csosn(codigo),
                modBC = icms.ModBC.HasValue ? ModBC(icms.ModBC) : null,
                vBC = icms.VBC,
                pRedBC = icms.PRedBC,
                pICMS = icms.PIcms,
                vICMS = icms.VIcms,
                modBCST = icms.ModBCST.HasValue ? ModBCST(icms.ModBCST) : null,
                pMVAST = icms.PMvaST,
                pRedBCST = icms.PRedBCST,
                vBCST = icms.VBCST,
                pICMSST = icms.PIcmsST,
                vICMSST = icms.VIcmsST,
                vBCFCPST = icms.VBCFcpST,
                pFCPST = icms.PFcpST,
                vFCPST = icms.VFcpST,
                pCredSN = icms.PCredSn,
                vCredICMSSN = icms.VCredIcmsSn
            },

            _ => throw new ArgumentException($"Grupo de ICMS sem traducao: {icms.Situacao.Grupo}")
        };
    }

    // ------------------------------------------------------------------------------- PIS

    public static PISBasico MontarPis(PisItem pis)
    {
        var cst = Codigo<CSTPIS>("pis", pis.Situacao.Codigo);
        var codigo = pis.Situacao.Codigo;

        return pis.Situacao.Forma switch
        {
            FormaApuracaoContribuicao.NaoTributada => new PISNT { CST = cst },

            FormaApuracaoContribuicao.Percentual => new PISAliq
            {
                CST = cst,
                vBC = Exigir(pis.VBC, "vBC", codigo),
                pPIS = Exigir(pis.PPis, "pPIS", codigo),
                vPIS = Exigir(pis.VPis, "vPIS", codigo)
            },

            FormaApuracaoContribuicao.Quantidade => new PISQtde
            {
                CST = cst,
                qBCProd = Exigir(pis.QBCProd, "qBCProd", codigo),
                vAliqProd = Exigir(pis.VAliqProd, "vAliqProd", codigo),
                vPIS = Exigir(pis.VPis, "vPIS", codigo)
            },

            // Em "outras operacoes" o layout aceita as duas formas; vale o par que veio.
            _ => pis.TemQuantidade
                ? new PISOutr
                {
                    CST = cst,
                    qBCProd = pis.QBCProd,
                    vAliqProd = pis.VAliqProd,
                    vPIS = pis.VPis
                }
                : new PISOutr
                {
                    CST = cst,
                    vBC = pis.VBC,
                    pPIS = pis.PPis,
                    vPIS = pis.VPis
                }
        };
    }

    // ---------------------------------------------------------------------------- COFINS

    public static COFINSBasico MontarCofins(CofinsItem cofins)
    {
        var cst = Codigo<CSTCOFINS>("cofins", cofins.Situacao.Codigo);
        var codigo = cofins.Situacao.Codigo;

        return cofins.Situacao.Forma switch
        {
            FormaApuracaoContribuicao.NaoTributada => new COFINSNT { CST = cst },

            FormaApuracaoContribuicao.Percentual => new COFINSAliq
            {
                CST = cst,
                vBC = Exigir(cofins.VBC, "vBC", codigo),
                pCOFINS = Exigir(cofins.PCofins, "pCOFINS", codigo),
                vCOFINS = Exigir(cofins.VCofins, "vCOFINS", codigo)
            },

            FormaApuracaoContribuicao.Quantidade => new COFINSQtde
            {
                CST = cst,
                qBCProd = Exigir(cofins.QBCProd, "qBCProd", codigo),
                vAliqProd = Exigir(cofins.VAliqProd, "vAliqProd", codigo),
                vCOFINS = Exigir(cofins.VCofins, "vCOFINS", codigo)
            },

            _ => cofins.TemQuantidade
                ? new COFINSOutr
                {
                    CST = cst,
                    qBCProd = cofins.QBCProd,
                    vAliqProd = cofins.VAliqProd,
                    vCOFINS = cofins.VCofins
                }
                : new COFINSOutr
                {
                    CST = cst,
                    vBC = cofins.VBC,
                    pCOFINS = cofins.PCofins,
                    vCOFINS = cofins.VCofins
                }
        };
    }

    // ------------------------------------------------------------------------------- IPI

    public static IPI MontarIpi(IpiItem ipi)
    {
        var cst = Codigo<CSTIPI>("ipi", ipi.Situacao.Codigo);

        IPIBasico tipo = ipi.Situacao.EhTributado
            ? new IPITrib
            {
                CST = cst,
                vBC = ipi.VBC,
                pIPI = ipi.PIpi,
                vIPI = ipi.VIpi
            }
            : new IPINT { CST = cst };

        // cEnq e ignorado na serializacao; quem vira a tag <cEnq> e ProxycEnq.
        return new IPI
        {
            ProxycEnq = ipi.CEnq,
            TipoIPI = tipo
        };
    }

    // -------------------------------------------------------------------------- mapeamentos

    public static OrigemMercadoria MapearOrigem(int origem)
    {
        if (origem is < 0 or > 8)
            throw new ArgumentException($"Origem da mercadoria invalida: {origem}");

        return (OrigemMercadoria)origem;
    }

    /// <summary>
    /// Os codigos de <c>modBC</c> do layout (0 a 3) sao os proprios valores do enum.
    /// Quando nao informado vale 3 — valor da operacao, o caso comum na NFC-e.
    /// </summary>
    private static DeterminacaoBaseIcms ModBC(int? modBC)
    {
        var codigo = modBC ?? (int)DeterminacaoBaseIcms.DbiValorOperacao;

        if (!Enum.IsDefined(typeof(DeterminacaoBaseIcms), codigo))
            throw new ArgumentException($"modBC invalido: {codigo}. Valores de 0 a 3");

        return (DeterminacaoBaseIcms)codigo;
    }

    /// <summary>
    /// Codigos de <c>modBCST</c> vao de 0 a 6. Quando nao informado vale 4 — margem de
    /// valor agregado, a forma usada na maioria das operacoes com ST.
    /// </summary>
    private static DeterminacaoBaseIcmsSt ModBCST(int? modBCST)
    {
        var codigo = modBCST ?? (int)DeterminacaoBaseIcmsSt.DbisMargemValorAgregado;

        if (!Enum.IsDefined(typeof(DeterminacaoBaseIcmsSt), codigo))
            throw new ArgumentException($"modBCST invalido: {codigo}. Valores de 0 a 6");

        return (DeterminacaoBaseIcmsSt)codigo;
    }

    private static Csticms Cst(string codigo) => Codigo<Csticms>("Cst", codigo);

    private static Csosnicms Csosn(string codigo) => Codigo<Csosnicms>("Csosn", codigo);

    /// <summary>
    /// Os enums de situacao tributaria da DFe.NET nomeiam cada valor como prefixo + codigo
    /// ("Cst00", "Csosn102", "pis01"). Como o codigo ja foi reconhecido no dominio, aqui
    /// basta reconstruir o nome — repetir a tabela inteira em switch so criaria um segundo
    /// lugar para ela ficar desatualizada.
    /// </summary>
    private static T Codigo<T>(string prefixo, string codigo) where T : struct, Enum
    {
        if (Enum.TryParse<T>(prefixo + codigo, out var valor))
            return valor;

        throw new ArgumentException($"Codigo '{codigo}' sem correspondente em {typeof(T).Name}");
    }

    /// <summary>
    /// Rede de seguranca: o campo e obrigatorio no grupo e a validacao de coerencia deveria
    /// ter barrado antes. Se chegou aqui nulo, e bug do motor — e melhor falhar dizendo qual
    /// campo do que emitir XML com zero no lugar.
    /// </summary>
    private static decimal Exigir(decimal? valor, string tag, string situacao)
    {
        return valor ?? throw new ArgumentException(
            $"Situacao tributaria {situacao}: campo {tag} e obrigatorio e nao foi informado");
    }
}
