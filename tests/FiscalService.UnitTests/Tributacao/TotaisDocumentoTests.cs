using FiscalService.Domain.Entities;
using FiscalService.Domain.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Tributacao;

/// <summary>
/// Os totais do documento sao somados dos itens. Antes disto o adapter escrevia zero em todo
/// campo de imposto — acertava por coincidencia, porque toda NFC-e emitida usava CSOSN 102.
/// Estes testes fixam as duas metades: a soma que passou a existir e o zero que continua certo.
/// </summary>
public class TotaisDocumentoTests
{
    private static ItemFiscal Item(int numero, decimal quantidade, decimal valorUnitario, ImpostoItem imposto)
        => new(numero, $"PROD-{numero}", $"Produto {numero}", "22021000", "5102", "UN",
            quantidade, valorUnitario, imposto);

    [Fact]
    public void Icms_destacado_soma_base_e_valor()
    {
        var itens = new[]
        {
            Item(1, 1m, 100m, QuadroTributario.Completo(icms: new IcmsItem
            {
                Situacao = SituacaoIcms.Criar("00"),
                Origem = 0,
                ModBC = 3,
                VBC = 100.00m,
                PIcms = 18.00m,
                VIcms = 18.00m
            })),
            Item(2, 1m, 50m, QuadroTributario.Completo(icms: new IcmsItem
            {
                Situacao = SituacaoIcms.Criar("00"),
                Origem = 0,
                ModBC = 3,
                VBC = 50.00m,
                PIcms = 18.00m,
                VIcms = 9.00m
            }))
        };

        var totais = TotaisDocumento.Somar(itens);

        totais.VBC.Should().Be(150.00m);
        totais.VIcms.Should().Be(27.00m);
        totais.VProd.Should().Be(150.00m);
        totais.VNF.Should().Be(150.00m, "o ICMS proprio ja esta dentro do preco e nao soma ao total da nota");
    }

    [Fact]
    public void Contribuicoes_sao_somadas_separadamente()
    {
        var itens = new[]
        {
            Item(1, 2m, 100m, QuadroTributario.Completo(
                pis: new PisItem
                {
                    Situacao = SituacaoPis.Criar("01"),
                    VBC = 200.00m,
                    PPis = 1.65m,
                    VPis = 3.30m
                },
                cofins: new CofinsItem
                {
                    Situacao = SituacaoCofins.Criar("01"),
                    VBC = 200.00m,
                    PCofins = 7.60m,
                    VCofins = 15.20m
                }))
        };

        var totais = TotaisDocumento.Somar(itens);

        totais.VPis.Should().Be(3.30m);
        totais.VCofins.Should().Be(15.20m);
    }

    [Fact]
    public void Substituicao_tributaria_soma_ao_total_da_nota()
    {
        var itens = new[]
        {
            Item(1, 1m, 100m, QuadroTributario.Completo(icms: new IcmsItem
            {
                Situacao = SituacaoIcms.Criar("10"),
                Origem = 0,
                ModBC = 3,
                VBC = 100.00m,
                PIcms = 18.00m,
                VIcms = 18.00m,
                ModBCST = 4,
                VBCST = 140.00m,
                PIcmsST = 18.00m,
                VIcmsST = 7.20m
            }))
        };

        var totais = TotaisDocumento.Somar(itens);

        totais.VBCST.Should().Be(140.00m);
        totais.VST.Should().Be(7.20m);
        totais.VNF.Should().Be(107.20m, "a ST devida nesta operacao e cobrada por fora e entra no total");
    }

    [Fact]
    public void Substituicao_retida_anteriormente_nao_entra_nos_totais()
    {
        var itens = new[]
        {
            Item(1, 1m, 100m, QuadroTributario.Completo(icms: new IcmsItem
            {
                Situacao = SituacaoIcms.Criar("500"),
                Origem = 0,
                VBCSTRet = 140.00m,
                VIcmsSTRet = 25.20m
            }))
        };

        var totais = TotaisDocumento.Somar(itens);

        totais.VBCST.Should().Be(0m);
        totais.VST.Should().Be(0m);
        totais.VNF.Should().Be(100.00m,
            "a ST retida ja foi recolhida por outro contribuinte e ja esta dentro do preco");
    }

    [Fact]
    public void Ipi_soma_ao_total_da_nota()
    {
        var itens = new[]
        {
            Item(1, 1m, 100m, QuadroTributario.Completo(ipi: new IpiItem
            {
                Situacao = SituacaoIpi.Criar("50"),
                VBC = 100.00m,
                PIpi = 10.00m,
                VIpi = 10.00m,
                CEnq = "999"
            }))
        };

        var totais = TotaisDocumento.Somar(itens);

        totais.VIpi.Should().Be(10.00m);
        totais.VNF.Should().Be(110.00m);
    }

    [Fact]
    public void Csosn_102_mantem_todos_os_totais_de_imposto_em_zero()
    {
        var itens = new[]
        {
            Item(1, 2m, 14m, QuadroTributario.Completo()),
            Item(2, 1m, 28m, QuadroTributario.Completo()),
            Item(3, 2m, 7m, QuadroTributario.Completo())
        };

        var totais = TotaisDocumento.Somar(itens);

        totais.VBC.Should().Be(0m);
        totais.VIcms.Should().Be(0m);
        totais.VST.Should().Be(0m);
        totais.VFcp.Should().Be(0m);
        totais.VPis.Should().Be(0m);
        totais.VCofins.Should().Be(0m);
        totais.VIpi.Should().Be(0m);
        totais.VProd.Should().Be(70.00m);
        totais.VNF.Should().Be(70.00m, "e exatamente a nota que foi autorizada em homologacao");
    }
}
