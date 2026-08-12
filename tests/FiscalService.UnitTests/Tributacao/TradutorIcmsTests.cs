using FiscalService.Domain.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// Um teste por situacao tributaria de ICMS aceita, conferindo o grupo XML gerado.
///
/// A afirmacao e sempre sobre o XML, nao sobre o objeto da DFe.NET: e o XML que a SEFAZ
/// recebe e o contador escritura, e um grupo montado com o objeto certo ainda pode serializar
/// errado.
/// </summary>
public class TradutorIcmsTests
{
    [Theory]
    // Regime Normal
    [InlineData("00", "ICMS00")]
    [InlineData("10", "ICMS10")]
    [InlineData("20", "ICMS20")]
    [InlineData("30", "ICMS30")]
    [InlineData("40", "ICMS40")]
    [InlineData("41", "ICMS40")]
    [InlineData("50", "ICMS40")]
    [InlineData("51", "ICMS51")]
    [InlineData("60", "ICMS60")]
    [InlineData("70", "ICMS70")]
    [InlineData("90", "ICMS90")]
    // Simples Nacional
    [InlineData("101", "ICMSSN101")]
    [InlineData("102", "ICMSSN102")]
    [InlineData("103", "ICMSSN102")]
    [InlineData("201", "ICMSSN201")]
    [InlineData("202", "ICMSSN202")]
    [InlineData("203", "ICMSSN202")]
    [InlineData("300", "ICMSSN102")]
    [InlineData("400", "ICMSSN102")]
    [InlineData("500", "ICMSSN500")]
    [InlineData("900", "ICMSSN900")]
    public void Cada_situacao_gera_o_grupo_do_layout(string situacao, string grupoEsperado)
    {
        var xml = QuadroTributario.Xml(QuadroTributario.Completo(QuadroTributario.Icms(situacao)));

        xml.Elemento(grupoEsperado).Should().NotBeNull(
            $"a situacao {situacao} corresponde ao grupo {grupoEsperado}");

        // O codigo vai no XML como veio, sem o motor reescrever.
        var grupo = xml.Elemento(grupoEsperado)!;
        var codigo = grupo.Elemento("CST")?.Value ?? grupo.Elemento("CSOSN")?.Value;
        codigo.Should().Be(situacao);
    }

    [Fact]
    public void Cst_00_leva_base_aliquota_e_valor_exatamente_como_vieram()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 250.00m,
            PIcms = 12.00m,
            VIcms = 30.00m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMS00")!;

        grupo.Texto("modBC").Should().Be("3");
        grupo.Texto("vBC").Should().Be("250.00");
        grupo.Texto("pICMS").Should().Be("12.0000");
        grupo.Texto("vICMS").Should().Be("30.00");
    }

    [Fact]
    public void Csosn_101_leva_o_credito_que_o_destinatario_aproveita()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("101"),
            Origem = 0,
            PCredSn = 2.83m,
            VCredIcmsSn = 2.83m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMSSN101")!;

        grupo.Texto("pCredSN").Should().Be("2.8300");
        grupo.Texto("vCredICMSSN").Should().Be("2.83");
    }

    [Fact]
    public void Csosn_102_gera_o_grupo_apenas_com_origem_e_csosn()
    {
        var icms = new IcmsItem { Situacao = SituacaoIcms.Criar("102"), Origem = 0 };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMSSN102")!;

        grupo.Elements().Select(e => e.Name.LocalName)
            .Should().Equal("orig", "CSOSN");
    }

    [Fact]
    public void Cst_10_leva_icms_proprio_e_st_no_mesmo_grupo()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("10"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            PIcms = 18.00m,
            VIcms = 18.00m,
            ModBCST = 4,
            PMvaST = 40.00m,
            VBCST = 140.00m,
            PIcmsST = 18.00m,
            VIcmsST = 7.20m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMS10")!;

        grupo.Texto("vICMS").Should().Be("18.00");
        grupo.Texto("pMVAST").Should().Be("40.0000");
        grupo.Texto("vBCST").Should().Be("140.00");
        grupo.Texto("vICMSST").Should().Be("7.20");
    }

    [Fact]
    public void Cst_20_leva_a_reducao_de_base()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("20"),
            Origem = 0,
            ModBC = 3,
            PRedBC = 33.33m,
            VBC = 66.67m,
            PIcms = 18.00m,
            VIcms = 12.00m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMS20")!;

        grupo.Texto("pRedBC").Should().Be("33.3300");
        grupo.Texto("vBC").Should().Be("66.67");
    }

    [Fact]
    public void Cst_60_leva_a_st_retida_anteriormente()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("60"),
            Origem = 0,
            VBCSTRet = 180.00m,
            VIcmsSTRet = 32.40m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMS60")!;

        grupo.Texto("vBCSTRet").Should().Be("180.00");
        grupo.Texto("vICMSSTRet").Should().Be("32.40");
    }

    [Fact]
    public void Fundo_de_combate_a_pobreza_entra_no_grupo_quando_informado()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            PIcms = 18.00m,
            VIcms = 18.00m,
            PFcp = 2.00m,
            VFcp = 2.00m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMS00")!;

        grupo.Texto("pFCP").Should().Be("2.0000");
        grupo.Texto("vFCP").Should().Be("2.00");
    }

    [Fact]
    public void Fundo_de_combate_a_pobreza_da_st_entra_no_grupo_de_st()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("202"),
            Origem = 0,
            ModBCST = 4,
            VBCST = 150.00m,
            PIcmsST = 18.00m,
            VIcmsST = 27.00m,
            VBCFcpST = 150.00m,
            PFcpST = 2.00m,
            VFcpST = 3.00m
        };

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMSSN202")!;

        grupo.Texto("vBCFCPST").Should().Be("150.00");
        grupo.Texto("pFCPST").Should().Be("2.0000");
        grupo.Texto("vFCPST").Should().Be("3.00");
    }

    [Fact]
    public void Origem_da_mercadoria_vai_para_o_xml()
    {
        var icms = QuadroTributario.Icms("102", origem: 5);

        var grupo = QuadroTributario.Xml(QuadroTributario.Completo(icms)).Elemento("ICMSSN102")!;

        grupo.Texto("orig").Should().Be("5");
    }

    [Fact]
    public void Origem_fora_da_tabela_e_recusada()
    {
        var icms = new IcmsItem { Situacao = SituacaoIcms.Criar("102"), Origem = 9 };

        var erro = Assert.Throws<ArgumentException>(
            () => QuadroTributario.Xml(QuadroTributario.Completo(icms)));

        erro.Message.Should().Contain("Origem da mercadoria invalida");
    }
}
