using System.Text;
using DFe.Classes.Entidades;
using DFe.Utils;
using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using FiscalService.Domain.Enums;
using FiscalService.Infrastructure.Danfe;
using FiscalService.Infrastructure.DFe;
using FluentAssertions;
using NFe.Classes;
using NFe.Classes.Protocolo;
using Xunit;

namespace FiscalService.UnitTests.Nfe;

/// <summary>
/// O DANFE do modelo 55 sai em HTML, nao em PDF: o layout retrato pronto da Zeus depende de
/// System.Drawing.Common, que e Windows-only no .NET 8, e o motor roda em contentor Linux.
/// Ver D6 no design da change.
///
/// O XML de entrada e montado aqui mesmo, a partir do payload do recorte — usar um arquivo
/// fixo esconderia a divergencia no dia em que a montagem mudar.
/// </summary>
public class DanfeNfeTests
{
    private static string XmlAutorizadoDeExemplo()
    {
        var nfe = NfeMapper.ToDomain(PayloadNfe.Valido());
        var documento = DFeNetAdapter.MontarNfe55(nfe, Ambiente.Producao, Estado.MG);

        var proc = new nfeProc
        {
            versao = "4.00",
            NFe = documento,
            protNFe = new protNFe
            {
                versao = "4.00",
                infProt = new infProt
                {
                    tpAmb = DFe.Classes.Flags.TipoAmbiente.Producao,
                    chNFe = documento.infNFe.Id![3..],
                    dhRecbto = DateTimeOffset.Now,
                    nProt = "131260000762680",
                    cStat = 100,
                    xMotivo = "Autorizado o uso da NF-e"
                }
            }
        };

        return FuncoesXml.ClasseParaXmlString(proc);
    }

    [Fact]
    public void Danfe_do_modelo_55_e_gerado_e_se_declara_html()
    {
        var danfe = new HtmlDanfeNfeGenerator().Gerar(XmlAutorizadoDeExemplo());

        danfe.ContentType.Should().Be(DanfeGerado.Html);
        danfe.Conteudo.Should().NotBeEmpty();
    }

    [Fact]
    public void Danfe_traz_os_dados_do_documento()
    {
        var danfe = new HtmlDanfeNfeGenerator().Gerar(XmlAutorizadoDeExemplo());
        var html = Encoding.UTF8.GetString(danfe.Conteudo);

        // Sem distinguir caixa: o layout do DANFE decide como grafar os nomes, e afirmar
        // sobre isso seria testar a biblioteca, nao a integracao com ela.
        html.Should().ContainEquivalentOf("Sal e Fogo Braga LTDA", "o emitente aparece no DANFE");
        html.Should().ContainEquivalentOf("Construtora Norte Mineira LTDA", "o destinatario tambem");
        html.Should().Contain("131260000762680", "o protocolo de autorizacao e obrigatorio no DANFE");
    }

    [Fact]
    public void Xml_vazio_e_recusado()
    {
        var gerador = new HtmlDanfeNfeGenerator();

        var erro = Assert.Throws<ArgumentException>(() => gerador.Gerar("  "));

        erro.ParamName.Should().Be("xmlAutorizado");
    }
}
