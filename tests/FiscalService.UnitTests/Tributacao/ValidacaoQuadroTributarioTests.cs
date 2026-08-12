using FiscalService.Domain.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// O motor recusa quadro que nao consegue montar corretamente — e a mensagem tem que dizer
/// o que corrigir, porque quem le e o desenvolvedor do backend depois que a emissao falhou.
/// </summary>
public class ValidacaoQuadroTributarioTests
{
    [Fact]
    public void Quadro_coerente_passa_sem_erro()
    {
        var erros = ValidacaoQuadroTributario.Validar(
            QuadroTributario.Completo(QuadroTributario.Icms("00")));

        erros.Should().BeEmpty();
    }

    // ------------------------------------------------------------------- campo faltando

    [Fact]
    public void Cst_que_exige_valores_chegando_sem_eles_e_recusado_dizendo_quais()
    {
        var icms = new IcmsItem { Situacao = SituacaoIcms.Criar("00"), Origem = 0 };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().ContainSingle()
            .Which.Should().Contain("ICMS CST 00")
            .And.Contain("modBC")
            .And.Contain("vBC")
            .And.Contain("pICMS")
            .And.Contain("vICMS");
    }

    [Fact]
    public void Base_sem_aliquota_e_recusada()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            VIcms = 18.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().Contain(e => e.Contains("pICMS"));
    }

    [Fact]
    public void Situacao_de_st_sem_os_campos_de_st_e_recusada_indicando_os_exigidos()
    {
        // CSOSN 202 e substituicao tributaria: sem os campos de ST nao ha o que montar.
        var icms = new IcmsItem { Situacao = SituacaoIcms.Criar("202"), Origem = 0 };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().ContainSingle()
            .Which.Should().Contain("ICMS CSOSN 202")
            .And.Contain("modBCST")
            .And.Contain("vBCST")
            .And.Contain("pICMSST")
            .And.Contain("vICMSST");
    }

    [Fact]
    public void Base_de_st_sem_aliquota_de_st_e_recusada_mesmo_no_grupo_opcional()
    {
        // Em CSOSN 900 a parte de ST e opcional — mas se vier, vem inteira.
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("900"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            PIcms = 18.00m,
            VIcms = 18.00m,
            VBCST = 150.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().Contain(e => e.Contains("vBCST") && e.Contains("pICMSST"));
    }

    [Fact]
    public void Fundo_de_combate_a_pobreza_pela_metade_e_recusado()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            PIcms = 18.00m,
            VIcms = 18.00m,
            PFcp = 2.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().Contain(e => e.Contains("pFCP") && e.Contains("vFCP"));
    }

    // --------------------------------------------------------------------- valor divergente

    [Fact]
    public void Valor_de_icms_que_nao_fecha_com_base_por_aliquota_e_recusado()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            PIcms = 18.00m,
            VIcms = 10.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().ContainSingle()
            .Which.Should().Contain("vICMS informado (10,00)")
            .And.Contain("vBC x pICMS")
            .And.Contain("18,00");
    }

    [Fact]
    public void Diferenca_de_um_centavo_e_arredondamento_e_passa()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 33.33m,
            PIcms = 18.00m,
            VIcms = 6.00m // 33,33 x 18% = 5,9994 -> 6,00
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().BeEmpty();
    }

    [Fact]
    public void Valor_de_pis_que_nao_fecha_e_recusado()
    {
        var pis = new PisItem
        {
            Situacao = SituacaoPis.Criar("01"),
            VBC = 100.00m,
            PPis = 1.65m,
            VPis = 5.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(pis: pis));

        erros.Should().ContainSingle()
            .Which.Should().Contain("PIS CST 01").And.Contain("vBC x pPIS");
    }

    [Fact]
    public void Valor_de_cofins_por_quantidade_que_nao_fecha_e_recusado()
    {
        var cofins = new CofinsItem
        {
            Situacao = SituacaoCofins.Criar("03"),
            QBCProd = 10.0000m,
            VAliqProd = 0.5000m,
            VCofins = 9.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(cofins: cofins));

        erros.Should().ContainSingle()
            .Which.Should().Contain("qBCProd x vAliqProd");
    }

    // --------------------------------------------------------------------- o motor nao calcula

    [Fact]
    public void Base_e_aliquota_sem_o_valor_do_imposto_sao_recusadas_em_vez_de_calculadas()
    {
        var icms = new IcmsItem
        {
            Situacao = SituacaoIcms.Criar("00"),
            Origem = 0,
            ModBC = 3,
            VBC = 100.00m,
            PIcms = 18.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(icms));

        erros.Should().ContainSingle().Which.Should().Contain("vICMS");
    }

    // ------------------------------------------------------------------------ PIS e COFINS

    [Fact]
    public void Contribuicao_nao_tributada_com_valores_e_recusada()
    {
        var pis = new PisItem
        {
            Situacao = SituacaoPis.Criar("07"),
            VBC = 100.00m,
            PPis = 1.65m,
            VPis = 1.65m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(pis: pis));

        erros.Should().ContainSingle()
            .Which.Should().Contain("PIS CST 07").And.Contain("apenas o CST");
    }

    [Fact]
    public void Outras_operacoes_sem_nenhuma_das_duas_formas_sao_recusadas()
    {
        var pis = new PisItem { Situacao = SituacaoPis.Criar("49") };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(pis: pis));

        erros.Should().ContainSingle()
            .Which.Should().Contain("percentual").And.Contain("quantidade");
    }

    [Fact]
    public void Outras_operacoes_com_as_duas_formas_ao_mesmo_tempo_sao_recusadas()
    {
        var pis = new PisItem
        {
            Situacao = SituacaoPis.Criar("49"),
            VBC = 100.00m,
            PPis = 1.65m,
            QBCProd = 10.00m,
            VAliqProd = 0.16m,
            VPis = 1.65m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(pis: pis));

        erros.Should().ContainSingle()
            .Which.Should().Contain("por percentual ou por quantidade");
    }

    // -------------------------------------------------------------------------------- IPI

    [Fact]
    public void Ipi_tributado_sem_valores_e_recusado()
    {
        var ipi = new IpiItem { Situacao = SituacaoIpi.Criar("50") };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(ipi: ipi));

        erros.Should().ContainSingle()
            .Which.Should().Contain("IPI CST 50")
            .And.Contain("vBC").And.Contain("pIPI").And.Contain("vIPI");
    }

    [Fact]
    public void Ipi_nao_tributado_com_valores_e_recusado()
    {
        var ipi = new IpiItem
        {
            Situacao = SituacaoIpi.Criar("53"),
            VBC = 100.00m,
            PIpi = 5.00m,
            VIpi = 5.00m
        };

        var erros = ValidacaoQuadroTributario.Validar(QuadroTributario.Completo(ipi: ipi));

        erros.Should().ContainSingle().Which.Should().Contain("apenas o CST");
    }

    // ------------------------------------------------------------- situacao inexistente

    [Theory]
    [InlineData("999")]
    [InlineData("11")]
    [InlineData("")]
    [InlineData("abc")]
    public void Situacao_de_icms_inexistente_nao_e_reconhecida(string codigo)
    {
        SituacaoIcms.TryCriar(codigo, out _).Should().BeFalse();
    }

    [Fact]
    public void Situacao_de_icms_inexistente_lista_os_codigos_validos_na_mensagem()
    {
        SituacaoIcms.MensagemInvalida("999")
            .Should().Contain("999").And.Contain("CST").And.Contain("CSOSN").And.Contain("102");
    }

    [Theory]
    [InlineData("0", "00")]
    [InlineData(" 40 ", "40")]
    [InlineData("102", "102")]
    public void Codigo_e_normalizado_antes_de_ser_reconhecido(string informado, string esperado)
    {
        SituacaoIcms.Criar(informado).Codigo.Should().Be(esperado);
    }

    [Fact]
    public void O_proprio_codigo_diz_o_regime_sem_precisar_do_crt()
    {
        SituacaoIcms.Criar("102").EhSimplesNacional.Should().BeTrue();
        SituacaoIcms.Criar("00").EhSimplesNacional.Should().BeFalse();
    }
}
