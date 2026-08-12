using FiscalService.Domain.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// PIS, COFINS e IPI. O caso que mais importa aqui e o primeiro: antes desta change toda
/// nota saia com PIS e COFINS em CST 07, independentemente do produto.
/// </summary>
public class TradutorContribuicoesTests
{
    [Fact]
    public void Pis_e_cofins_saem_com_o_cst_que_veio_no_payload()
    {
        var quadro = QuadroTributario.Completo(
            pis: new PisItem
            {
                Situacao = SituacaoPis.Criar("04"),
            },
            cofins: new CofinsItem
            {
                Situacao = SituacaoCofins.Criar("04")
            });

        var xml = QuadroTributario.Xml(quadro);

        xml.Elemento("PISNT")!.Texto("CST").Should().Be("04");
        xml.Elemento("COFINSNT")!.Texto("CST").Should().Be("04");
    }

    [Fact]
    public void Pis_por_percentual_gera_PISAliq()
    {
        var quadro = QuadroTributario.Completo(
            pis: new PisItem
            {
                Situacao = SituacaoPis.Criar("01"),
                VBC = 100.00m,
                PPis = 1.65m,
                VPis = 1.65m
            });

        var grupo = QuadroTributario.Xml(quadro).Elemento("PISAliq")!;

        grupo.Texto("CST").Should().Be("01");
        grupo.Texto("vBC").Should().Be("100.00");
        grupo.Texto("pPIS").Should().Be("1.6500");
        grupo.Texto("vPIS").Should().Be("1.65");
    }

    [Fact]
    public void Cofins_por_percentual_gera_COFINSAliq()
    {
        var quadro = QuadroTributario.Completo(
            cofins: new CofinsItem
            {
                Situacao = SituacaoCofins.Criar("01"),
                VBC = 100.00m,
                PCofins = 7.60m,
                VCofins = 7.60m
            });

        var grupo = QuadroTributario.Xml(quadro).Elemento("COFINSAliq")!;

        grupo.Texto("vBC").Should().Be("100.00");
        grupo.Texto("pCOFINS").Should().Be("7.6000");
        grupo.Texto("vCOFINS").Should().Be("7.60");
    }

    [Fact]
    public void Pis_por_quantidade_gera_PISQtde_sem_exigir_aliquota_percentual()
    {
        var quadro = QuadroTributario.Completo(
            pis: new PisItem
            {
                Situacao = SituacaoPis.Criar("03"),
                QBCProd = 12.0000m,
                VAliqProd = 0.5000m,
                VPis = 6.00m
            });

        var xml = QuadroTributario.Xml(quadro);
        var grupo = xml.Elemento("PISQtde")!;

        grupo.Texto("qBCProd").Should().Be("12.0000");
        grupo.Texto("vAliqProd").Should().Be("0.5000");
        grupo.Texto("vPIS").Should().Be("6.00");
        grupo.Elemento("pPIS").Should().BeNull("a apuracao por quantidade nao tem aliquota percentual");
    }

    [Fact]
    public void Cofins_por_quantidade_gera_COFINSQtde()
    {
        var quadro = QuadroTributario.Completo(
            cofins: new CofinsItem
            {
                Situacao = SituacaoCofins.Criar("03"),
                QBCProd = 12.0000m,
                VAliqProd = 2.3000m,
                VCofins = 27.60m
            });

        var grupo = QuadroTributario.Xml(quadro).Elemento("COFINSQtde")!;

        grupo.Texto("qBCProd").Should().Be("12.0000");
        grupo.Texto("vAliqProd").Should().Be("2.3000");
        grupo.Texto("vCOFINS").Should().Be("27.60");
    }

    [Fact]
    public void Outras_operacoes_por_percentual_geram_PISOutr_com_base_e_aliquota()
    {
        var quadro = QuadroTributario.Completo(
            pis: new PisItem
            {
                Situacao = SituacaoPis.Criar("49"),
                VBC = 0m,
                PPis = 0m,
                VPis = 0m
            });

        var grupo = QuadroTributario.Xml(quadro).Elemento("PISOutr")!;

        grupo.Texto("CST").Should().Be("49");
        grupo.Texto("vBC").Should().Be("0.00");
        grupo.Elemento("qBCProd").Should().BeNull();
    }

    [Fact]
    public void Outras_operacoes_por_quantidade_geram_PISOutr_por_quantidade()
    {
        var quadro = QuadroTributario.Completo(
            pis: new PisItem
            {
                Situacao = SituacaoPis.Criar("99"),
                QBCProd = 10.0000m,
                VAliqProd = 0.1000m,
                VPis = 1.00m
            });

        var grupo = QuadroTributario.Xml(quadro).Elemento("PISOutr")!;

        grupo.Texto("qBCProd").Should().Be("10.0000");
        grupo.Elemento("vBC").Should().BeNull();
    }

    [Fact]
    public void Ipi_so_entra_no_xml_quando_informado()
    {
        var semIpi = QuadroTributario.Xml(QuadroTributario.Completo());

        semIpi.Elemento("IPI").Should().BeNull();
    }

    [Fact]
    public void Ipi_tributado_gera_IPITrib_com_base_aliquota_e_valor()
    {
        var quadro = QuadroTributario.Completo(
            ipi: new IpiItem
            {
                Situacao = SituacaoIpi.Criar("50"),
                VBC = 100.00m,
                PIpi = 5.00m,
                VIpi = 5.00m,
                CEnq = "999"
            });

        var xml = QuadroTributario.Xml(quadro);

        xml.Elemento("IPI")!.Texto("cEnq").Should().Be("999");

        var grupo = xml.Elemento("IPITrib")!;
        grupo.Texto("CST").Should().Be("50");
        grupo.Texto("vBC").Should().Be("100.00");
        grupo.Texto("pIPI").Should().Be("5.0000");
        grupo.Texto("vIPI").Should().Be("5.00");
    }

    [Fact]
    public void Ipi_nao_tributado_gera_IPINT_apenas_com_o_cst()
    {
        var quadro = QuadroTributario.Completo(
            ipi: new IpiItem { Situacao = SituacaoIpi.Criar("53") });

        var grupo = QuadroTributario.Xml(quadro).Elemento("IPINT")!;

        grupo.Elements().Select(e => e.Name.LocalName).Should().Equal("CST");
        grupo.Texto("CST").Should().Be("53");
    }
}
