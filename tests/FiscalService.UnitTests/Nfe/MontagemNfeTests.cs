using System.Xml.Linq;
using DFe.Classes.Entidades;
using DFe.Utils;
using FiscalService.Application.DTOs;
using FiscalService.Application.Mappers;
using FiscalService.Domain.Enums;
using FiscalService.Infrastructure.DFe;
using FiscalService.UnitTests.Tributacao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Nfe;

/// <summary>
/// Afirma sobre o XML que a SEFAZ receberia, nao sobre o objeto intermediario. A montagem e
/// exercitada sem certificado e sem rede: assinar e transmitir sao outra coisa.
/// </summary>
public class MontagemNfeTests
{
    private static XElement Montar(Action<Application.UseCases.EmitirNfe.EmitirNfeRequest>? ajustar = null,
        Ambiente ambiente = Ambiente.Producao)
    {
        var request = PayloadNfe.Valido();
        ajustar?.Invoke(request);

        var nfe = NfeMapper.ToDomain(request);
        var documento = DFeNetAdapter.MontarNfe55(nfe, ambiente, Estado.MG);

        return XDocument.Parse(FuncoesXml.ClasseParaXmlString(documento)).Root!;
    }

    [Fact]
    public void Identificacao_sai_no_modelo_55()
    {
        var xml = Montar();

        xml.Texto("mod").Should().Be("55");
        xml.Texto("tpNF").Should().Be("1");
        xml.Texto("finNFe").Should().Be("1");
        xml.Texto("idDest").Should().Be("1", "o recorte e operacao interna");
        xml.Texto("tpImp").Should().Be("1", "o DANFE do modelo 55 e retrato");
    }

    [Fact]
    public void Nfe_nao_leva_informacoes_suplementares()
    {
        var xml = Montar();

        xml.Elemento("infNFeSupl").Should().BeNull("QR Code e URL de consulta sao da NFC-e");
        xml.Elemento("qrCode").Should().BeNull();
    }

    [Fact]
    public void Destinatario_sai_completo_com_indicador_de_ie()
    {
        var xml = Montar();
        var dest = xml.Elemento("dest")!;

        dest.Texto("CNPJ").Should().Be(PayloadNfe.CnpjDestinatario);
        dest.Texto("indIEDest").Should().Be("1");
        dest.Texto("IE").Should().Be("0011223340012");
        dest.Texto("xLgr").Should().Be("Avenida Ovidio de Abreu");
        dest.Texto("UF").Should().Be("MG");
        dest.Texto("cMun").Should().Be("3143302");
    }

    [Fact]
    public void Nao_contribuinte_sai_sem_inscricao_estadual()
    {
        var xml = Montar(r => r.Destinatario = PayloadNfe.Destinatario(
            indicadorIe: (int)IndicadorIeDestinatario.NaoContribuinte,
            inscricaoEstadual: null));

        var dest = xml.Elemento("dest")!;

        dest.Texto("indIEDest").Should().Be("9");
        dest.Elemento("IE").Should().BeNull();
    }

    [Fact]
    public void Consumidor_final_reflete_o_que_o_backend_declarou()
    {
        Montar(r => r.ConsumidorFinal = false).Texto("indFinal").Should().Be("0");
        Montar(r => r.ConsumidorFinal = true).Texto("indFinal").Should().Be("1");
    }

    // ------------------------------------------------------------------ grupos opcionais

    [Fact]
    public void Sem_transporte_informado_a_nota_declara_ausencia_de_frete()
    {
        var xml = Montar();

        xml.Texto("modFrete").Should().Be("9");
        xml.Elemento("vol").Should().BeNull();
        xml.Elemento("transporta").Should().BeNull();
    }

    [Fact]
    public void Transporte_e_volumes_saem_quando_informados()
    {
        var xml = Montar(r => r.Transporte = new TransporteDto(
            Modalidade: (int)ModalidadeFrete.ContratacaoPorContaDoRemetente,
            Transportadora: new TransportadoraDto(
                PayloadNfe.CnpjDestinatario, "Transportadora Braga ME", "0011223340012",
                "Rua das Cargas, 50", "Montes Claros", "MG"),
            Veiculo: new VeiculoDto("HAB1234", "MG", "12345"),
            Volumes: new List<VolumeDto> { new(3, "CAIXA", "SF", "001", 12.5m, 13.2m) }));

        xml.Texto("modFrete").Should().Be("0");
        xml.Elemento("transporta")!.Texto("xNome").Should().Be("Transportadora Braga ME");
        xml.Elemento("veicTransp")!.Texto("placa").Should().Be("HAB1234");

        var volume = xml.Elemento("vol")!;
        volume.Texto("qVol").Should().Be("3");
        volume.Texto("esp").Should().Be("CAIXA");
        volume.Texto("pesoB").Should().Be("13.200");
    }

    [Fact]
    public void Cobranca_sai_com_uma_duplicata_por_parcela()
    {
        var xml = Montar(r => r.Cobranca = new CobrancaDto(
            "001", 100m, null, 100m,
            new List<DuplicataDto>
            {
                new("001/1", new DateTime(2026, 9, 13), 50m),
                new("001/2", new DateTime(2026, 10, 13), 50m)
            }));

        var cobr = xml.Elemento("cobr")!;

        cobr.Texto("nFat").Should().Be("001");
        cobr.Descendants().Count(e => e.Name.LocalName == "dup").Should().Be(2);
    }

    [Fact]
    public void Sem_cobranca_informada_o_grupo_nao_existe()
    {
        Montar().Elemento("cobr").Should().BeNull();
    }

    // ------------------------------------------------------------------ homologacao

    [Fact]
    public void Em_homologacao_o_nome_do_destinatario_e_substituido_mas_o_documento_nao()
    {
        var xml = Montar(ambiente: Ambiente.Homologacao);
        var dest = xml.Elemento("dest")!;

        dest.Texto("xNome").Should().Be("NF-E EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL");
        dest.Texto("CNPJ").Should().Be(PayloadNfe.CnpjDestinatario,
            "o texto obrigatorio da NT 2015/002 e do nome, nao do documento");
    }

    // ------------------------------------------------------------------ totais

    [Fact]
    public void Totais_refletem_os_itens()
    {
        var xml = Montar(r =>
        {
            r.Itens = new List<ItemNfceDto> { PayloadNfe.Item(valorUnitario: 100m), PayloadNfe.Item(2, valorUnitario: 50m) };
            r.Pagamentos = new List<PagamentoDto> { new("dinheiro", 150m) };
            r.ValorTotal = 150m;
        });

        // Escopado no ICMSTot de proposito: <vProd> tambem existe dentro de cada item, e o
        // primeiro que aparece no documento e o do item.
        var totais = xml.Elemento("ICMSTot")!;

        totais.Texto("vProd").Should().Be("150.00");
        totais.Texto("vNF").Should().Be("150.00");
        totais.Texto("vICMS").Should().Be("0.00", "CSOSN 102 nao destaca ICMS");
    }
}
