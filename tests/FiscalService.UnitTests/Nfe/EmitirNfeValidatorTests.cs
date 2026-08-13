using FiscalService.Application.UseCases.EmitirNfe;
using FiscalService.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Nfe;

/// <summary>
/// O recorte vigente — operacao interna, destinatario pessoa juridica, saida, finalidade
/// normal — vive aqui, como recusa nomeada. Cada caso fora dele precisa dizer o que
/// aconteceu: aceitar em silencio e montar um XML aproximado e o unico desfecho inaceitavel,
/// porque a SEFAZ autoriza e o contador escritura errado sem ter como perceber.
/// </summary>
public class EmitirNfeValidatorTests
{
    private static readonly EmitirNfeValidator Validador = new();

    private static IEnumerable<string> Erros(EmitirNfeRequest request)
        => Validador.Validate(request).Errors.Select(e => e.ErrorMessage);

    [Fact]
    public void Payload_do_recorte_e_aceito()
    {
        Validador.Validate(PayloadNfe.Valido()).IsValid.Should().BeTrue();
    }

    // ------------------------------------------------------------------ destinatario

    [Fact]
    public void Destinatario_pessoa_fisica_e_recusado_apontando_a_nfce()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario(cpfCnpj: "11144477735");

        Erros(request).Should().Contain(e => e.Contains("pessoa juridica") && e.Contains("NFC-e"));
    }

    [Fact]
    public void Destinatario_ausente_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = null!;

        Erros(request).Should().Contain("Destinatario e obrigatorio na NF-e modelo 55");
    }

    [Fact]
    public void Destinatario_sem_endereco_e_recusado_campo_a_campo()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario() with { Logradouro = "", Bairro = "", Cep = "" };

        var erros = Erros(request).ToList();

        erros.Should().Contain("Logradouro do destinatario e obrigatorio");
        erros.Should().Contain("Bairro do destinatario e obrigatorio");
        erros.Should().Contain("CEP do destinatario e obrigatorio");
    }

    // ------------------------------------------------------------------ indicador de IE

    [Fact]
    public void Contribuinte_sem_inscricao_estadual_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario(
            indicadorIe: (int)IndicadorIeDestinatario.Contribuinte,
            inscricaoEstadual: null);

        Erros(request).Should().Contain("Destinatario declarado contribuinte exige inscricao estadual");
    }

    [Fact]
    public void Nao_contribuinte_com_inscricao_estadual_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario(
            indicadorIe: (int)IndicadorIeDestinatario.NaoContribuinte,
            inscricaoEstadual: "0011223340012");

        Erros(request).Should().Contain(e => e.Contains("nao pode ter inscricao estadual"));
    }

    [Theory]
    [InlineData((int)IndicadorIeDestinatario.IsentoDeInscricao)]
    [InlineData((int)IndicadorIeDestinatario.NaoContribuinte)]
    public void Isento_e_nao_contribuinte_sao_aceitos_sem_inscricao_estadual(int indicador)
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario(indicadorIe: indicador, inscricaoEstadual: null);

        Validador.Validate(request).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Indicador_de_ie_fora_da_tabela_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario(indicadorIe: 5, inscricaoEstadual: null);

        Erros(request).Should().Contain(e => e.Contains("Indicador de IE invalido"));
    }

    // ------------------------------------------------------------------ operacao interna

    [Fact]
    public void Destinatario_em_outra_uf_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.Destinatario = PayloadNfe.Destinatario(uf: "SP");

        Erros(request).Should().Contain(e =>
            e.Contains("interestadual") && e.Contains("MG") && e.Contains("SP"));
    }

    [Fact]
    public void Cfop_interestadual_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.Itens = new() { PayloadNfe.Item(cfop: "6102") };

        Erros(request).Should().Contain(e => e.Contains("CFOP") && e.Contains("operacao interna"));
    }

    // ------------------------------------------------------------------ finalidade e tipo

    [Theory]
    [InlineData((int)FinalidadeNfe.Complementar, "complementar")]
    [InlineData((int)FinalidadeNfe.Ajuste, "ajuste")]
    [InlineData((int)FinalidadeNfe.Devolucao, "devolucao")]
    public void Finalidade_fora_do_recorte_e_recusada_nomeando_a_recebida(int finalidade, string nome)
    {
        var request = PayloadNfe.Valido();
        request.Finalidade = finalidade;

        Erros(request).Should().Contain(e => e.Contains("finalidade normal") && e.Contains(nome));
    }

    [Fact]
    public void Nota_de_entrada_e_recusada()
    {
        var request = PayloadNfe.Valido();
        request.TipoOperacao = (int)TipoOperacao.Entrada;

        Erros(request).Should().Contain("Apenas nota de saida e aceita no escopo atual");
    }

    // ------------------------------------------------------------------ CSC

    [Fact]
    public void Csc_informado_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.CodigoCsc = "qualquer-coisa";
        request.IdCsc = "1";

        var erros = Erros(request).ToList();

        erros.Should().Contain(e => e.Contains("CSC e exclusivo da NFC-e"));
        erros.Should().Contain(e => e.Contains("ID do CSC e exclusivo da NFC-e"));
    }

    // ------------------------------------------------------------------ valores

    [Fact]
    public void Total_divergente_da_soma_dos_itens_e_recusado()
    {
        var request = PayloadNfe.Valido();
        request.ValorTotal = 150m;

        Erros(request).Should().Contain(e => e.Contains("diverge da soma dos itens"));
    }

    [Fact]
    public void Duplicata_sem_valor_e_recusada()
    {
        var request = PayloadNfe.Valido();
        request.Cobranca = new Application.DTOs.CobrancaDto(
            "001", 100m, null, 100m,
            new List<Application.DTOs.DuplicataDto>
            {
                new("001", new DateTime(2026, 9, 13), 0m)
            });

        Erros(request).Should().Contain("Toda duplicata deve ter valor maior que zero");
    }
}
