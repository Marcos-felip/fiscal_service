using System.Xml.Linq;
using DFe.Classes.Entidades;
using DFe.Utils;
using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FiscalService.Application.UseCases.EmitirNfce;
using FiscalService.Domain.Enums;
using FiscalService.Infrastructure.DFe;
using FiscalService.UnitTests.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Nfe;

/// <summary>
/// A NFC-e ja estava no ar quando a NF-e chegou — inclusive com uma nota autorizada em
/// homologacao. Estes testes fixam o que a separa do modelo 55, para que a montagem nova nao
/// tenha vazado para a antiga: modelo, tipo de impressao, consumidor anonimo e totais zerados.
///
/// O QR Code nao entra aqui: ele e aplicado depois da montagem, a partir do certificado, e
/// nao ha como exercita-lo sem um PFX de verdade. O que se garante e que o caminho da NFC-e
/// continua sendo o dela.
/// </summary>
public class RegressaoNfceTests
{
    private static EmitirNfceRequest PayloadNfce() => new()
    {
        Emitente = PayloadNfe.Emitente(),
        Destinatario = null,
        Itens = new List<ItemNfceDto> { PayloadNfe.Item() },
        Pagamentos = new List<PagamentoDto> { new("dinheiro", 100m) },
        ValorTotal = 100m,
        CertificadoBase64 = PayloadNfe.CertificadoFalso,
        CertificadoSenha = "senha",
        CodigoCsc = "csc-de-teste",
        IdCsc = "1",
        Serie = 1,
        Numero = 1,
        Ambiente = "producao"
    };

    private static XElement Montar(EmitirNfceRequest request)
    {
        var nfce = NfceMapper.ToDomain(request);
        var documento = DFeNetAdapter.MontarNfe(nfce, Ambiente.Producao, Estado.MG);

        return XDocument.Parse(FuncoesXml.ClasseParaXmlString(documento)).Root!;
    }

    [Fact]
    public void Nfce_continua_no_modelo_65_com_impressao_de_cupom()
    {
        var xml = Montar(PayloadNfce());

        xml.Texto("mod").Should().Be("65");
        xml.Texto("tpImp").Should().Be("4", "4 e o DANFE NFC-e; a NF-e usa 1, retrato");
        xml.Texto("indFinal").Should().Be("1", "a NFC-e e sempre a consumidor final");
    }

    [Fact]
    public void Nfce_sem_consumidor_identificado_nao_leva_destinatario()
    {
        Montar(PayloadNfce()).Elemento("dest").Should().BeNull();
    }

    [Fact]
    public void Nfce_com_csosn_102_mantem_os_totais_zerados()
    {
        var totais = Montar(PayloadNfce()).Elemento("ICMSTot")!;

        totais.Texto("vBC").Should().Be("0.00");
        totais.Texto("vICMS").Should().Be("0.00");
        totais.Texto("vPIS").Should().Be("0.00");
        totais.Texto("vCOFINS").Should().Be("0.00");
        totais.Texto("vProd").Should().Be("100.00");
        totais.Texto("vNF").Should().Be("100.00");
    }

    [Fact]
    public void Nfce_continua_aceitando_o_csc_que_a_nfe_recusa()
    {
        var validador = new EmitirNfceValidator();

        validador.Validate(PayloadNfce()).IsValid.Should().BeTrue();
    }
}
