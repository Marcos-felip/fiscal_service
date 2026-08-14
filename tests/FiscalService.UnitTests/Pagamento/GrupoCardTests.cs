using System.Xml.Linq;
using DFe.Classes.Entidades;
using DFe.Utils;
using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FiscalService.Application.UseCases.EmitirNfce;
using FiscalService.Domain.Enums;
using FiscalService.Infrastructure.DFe;
using FiscalService.UnitTests.Nfe;
using FiscalService.UnitTests.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Pagamento;

/// <summary>
/// O layout exige o grupo `card` em todo pagamento eletronico, nao so em cartao.
///
/// Faltar o grupo e a rejeicao 391 — que fala em "dados do cartao de
/// credito/debito" mesmo quando o pagamento foi PIX, e por isso e dificil de
/// diagnosticar. Aconteceu em 13/08/2026 com a primeira venda paga em PIX, ja
/// depois de a numeracao ter sido consumida.
///
/// Informa-lo fora da lista tambem e rejeicao: nao da para simplificar mandando
/// sempre. Por isso cada forma tem teste dos dois lados.
/// </summary>
public class GrupoCardTests
{
    private static XElement MontarNfce(params (string tipo, decimal valor)[] pagamentos)
    {
        var request = new EmitirNfceRequest
        {
            Emitente = PayloadNfe.Emitente(),
            Itens = new List<ItemNfceDto> { PayloadNfe.Item(valorUnitario: 100m) },
            Pagamentos = pagamentos
                .Select(p => new PagamentoDto(p.tipo, p.valor))
                .ToList(),
            ValorTotal = 100m,
            CertificadoBase64 = PayloadNfe.CertificadoFalso,
            CertificadoSenha = "senha",
            CodigoCsc = "csc-de-teste",
            IdCsc = "1",
            Serie = 1,
            Numero = 1,
            Ambiente = "producao",
        };

        var nfce = NfceMapper.ToDomain(request);
        var documento = DFeNetAdapter.MontarNfe(nfce, Ambiente.Producao, Estado.MG);

        return XDocument.Parse(FuncoesXml.ClasseParaXmlString(documento)).Root!;
    }

    /// <summary>Detalhes de pagamento do XML, na ordem em que saíram.</summary>
    private static List<XElement> DetalhesDePagamento(XElement xml) =>
        xml.Descendants().Where(e => e.Name.LocalName == "detPag").ToList();

    private static bool TemGrupoCard(XElement detalhe) =>
        detalhe.Descendants().Any(e => e.Name.LocalName == "card");

    [Fact]
    public void Pix_leva_o_grupo_de_cartoes()
    {
        var detalhe = DetalhesDePagamento(MontarNfce(("pix", 100m))).Single();

        TemGrupoCard(detalhe).Should().BeTrue(
            "e o caso que a SEFAZ recusou com a rejeicao 391");
    }

    [Fact]
    public void Pix_declara_integracao_como_nao_integrada()
    {
        var detalhe = DetalhesDePagamento(MontarNfce(("pix", 100m))).Single();

        // 2 = nao integrado. Declarar 1 tornaria obrigatorios CNPJ da
        // credenciadora, bandeira e codigo de autorizacao.
        detalhe.Texto("tpIntegra").Should().Be("2");
        detalhe.Elemento("CNPJ").Should().BeNull();
        detalhe.Elemento("tBand").Should().BeNull();
        detalhe.Elemento("cAut").Should().BeNull();
    }

    [Theory]
    [InlineData("cartao_credito")]
    [InlineData("cartao_debito")]
    [InlineData("vale_alimentacao")]
    [InlineData("vale_refeicao")]
    [InlineData("vale_presente")]
    [InlineData("vale_combustivel")]
    [InlineData("boleto")]
    [InlineData("pix")]
    public void Formas_eletronicas_levam_o_grupo(string tipo)
    {
        var detalhe = DetalhesDePagamento(MontarNfce((tipo, 100m))).Single();

        TemGrupoCard(detalhe).Should().BeTrue();
    }

    [Theory]
    [InlineData("dinheiro")]
    [InlineData("cheque")]
    [InlineData("credito_loja")]
    [InlineData("sem_pagamento")]
    [InlineData("outro")]
    public void Formas_nao_eletronicas_nao_levam_o_grupo(string tipo)
    {
        var detalhe = DetalhesDePagamento(MontarNfce((tipo, 100m))).Single();

        TemGrupoCard(detalhe).Should().BeFalse(
            "informar o grupo fora da lista do layout e rejeicao por grupo indevido");
    }

    [Fact]
    public void Numa_venda_dividida_so_o_pagamento_eletronico_leva_o_grupo()
    {
        var detalhes = DetalhesDePagamento(MontarNfce(("dinheiro", 60m), ("pix", 40m)));

        detalhes.Should().HaveCount(2);
        TemGrupoCard(detalhes[0]).Should().BeFalse();
        TemGrupoCard(detalhes[1]).Should().BeTrue();
    }

    [Fact]
    public void Nfce_em_dinheiro_continua_com_o_xml_de_sempre()
    {
        var detalhe = DetalhesDePagamento(MontarNfce(("dinheiro", 100m))).Single();

        // A nota autorizada em homologacao em 13/08/2026 foi esta: sem grupo de
        // cartoes, tPag 01, a vista.
        detalhe.Texto("tPag").Should().Be("01");
        detalhe.Texto("indPag").Should().Be("0");
        TemGrupoCard(detalhe).Should().BeFalse();
    }
}
