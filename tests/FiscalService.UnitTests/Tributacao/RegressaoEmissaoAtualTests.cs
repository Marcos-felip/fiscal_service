using System.Xml.Linq;
using DFe.Utils;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Tributacao;
using FiscalService.Infrastructure.DFe;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// A emissao de NFC-e ja funcionava antes desta change, com CSOSN 102 e PIS/COFINS em CST 07.
/// Estes testes fixam o XML daquele caso: e a garantia de que o contrato tributario novo nao
/// mudou o que ja estava no ar.
///
/// O XML esperado esta escrito por extenso de proposito. Comparar contra outra funcao do
/// proprio motor so provaria que as duas concordam — e as duas podem estar erradas juntas.
/// </summary>
public class RegressaoEmissaoAtualTests
{
    [Fact]
    public void Csosn_102_sem_valores_gera_o_xml_de_sempre()
    {
        var quadro = QuadroTributario.Completo(
            icms: new IcmsItem { Situacao = SituacaoIcms.Criar("102"), Origem = 0 },
            pis: new PisItem { Situacao = SituacaoPis.Criar("07") },
            cofins: new CofinsItem { Situacao = SituacaoCofins.Criar("07") });

        var gerado = QuadroTributario.Xml(quadro);

        var esperado = XElement.Parse("""
            <imposto>
              <ICMS>
                <ICMSSN102>
                  <orig>0</orig>
                  <CSOSN>102</CSOSN>
                </ICMSSN102>
              </ICMS>
              <PIS>
                <PISNT>
                  <CST>07</CST>
                </PISNT>
              </PIS>
              <COFINS>
                <COFINSNT>
                  <CST>07</CST>
                </COFINSNT>
              </COFINS>
            </imposto>
            """);

        XNode.DeepEquals(gerado, esperado).Should().BeTrue(
            "o XML gerado foi:\n{0}", gerado);
    }

    [Fact]
    public void Cst_40_sem_valores_gera_o_xml_de_sempre()
    {
        var quadro = QuadroTributario.Completo(
            icms: new IcmsItem { Situacao = SituacaoIcms.Criar("40"), Origem = 0 },
            pis: new PisItem { Situacao = SituacaoPis.Criar("07") },
            cofins: new CofinsItem { Situacao = SituacaoCofins.Criar("07") });

        var gerado = QuadroTributario.Xml(quadro);

        var esperado = XElement.Parse("""
            <imposto>
              <ICMS>
                <ICMS40>
                  <orig>0</orig>
                  <CST>40</CST>
                </ICMS40>
              </ICMS>
              <PIS>
                <PISNT>
                  <CST>07</CST>
                </PISNT>
              </PIS>
              <COFINS>
                <COFINSNT>
                  <CST>07</CST>
                </COFINSNT>
              </COFINS>
            </imposto>
            """);

        XNode.DeepEquals(gerado, esperado).Should().BeTrue(
            "o XML gerado foi:\n{0}", gerado);
    }

    [Fact]
    public void O_item_leva_o_quadro_ate_o_xml()
    {
        var item = new NfceItem(
            numeroItem: 1,
            codigoProduto: "PROD-1",
            descricao: "Produto",
            ncm: "22021000",
            cfop: "5102",
            unidadeComercial: "UN",
            quantidade: 1m,
            valorUnitario: 10m,
            imposto: QuadroTributario.Completo(
                icms: new IcmsItem
                {
                    Situacao = SituacaoIcms.Criar("101"),
                    Origem = 0,
                    PCredSn = 2.50m,
                    VCredIcmsSn = 2.50m
                }));

        var xml = XDocument.Parse(FuncoesXml.ClasseParaXmlString(TradutorImposto.Montar(item))).Root!;

        xml.Elemento("ICMSSN101").Should().NotBeNull();
    }

    [Fact]
    public void Item_sem_quadro_tributario_nao_e_construivel()
    {
        var erro = Assert.Throws<ArgumentNullException>(() => new NfceItem(
            numeroItem: 1,
            codigoProduto: "PROD-1",
            descricao: "Produto",
            ncm: "22021000",
            cfop: "5102",
            unidadeComercial: "UN",
            quantidade: 1m,
            valorUnitario: 10m,
            imposto: null!));

        erro.ParamName.Should().Be("imposto");
    }
}
