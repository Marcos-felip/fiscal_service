using FiscalService.Application.UseCases.InutilizarNumeracao;
using FluentAssertions;
using Xunit;

namespace FiscalService.UnitTests.Eventos;

/// <summary>
/// Inutilizacao age sobre uma **faixa**, nao sobre um documento: nao tem chave
/// de acesso nem protocolo de evento, e o servico da SEFAZ e outro.
///
/// Serve para regularizar numero reservado que nunca sera usado — foi o que
/// aconteceu com a NF-e nº 1 em 13/08/2026, quando a criacao do documento
/// falhou depois de a numeracao ja ter sido consumida.
/// </summary>
public class InutilizarNumeracaoValidatorTests
{
    private static readonly InutilizarNumeracaoValidator Validador = new();

    private static InutilizarNumeracaoRequest Valido() => new()
    {
        Cnpj = "51720322000146",
        Ano = 2026,
        Modelo = 55,
        Serie = 1,
        NumeroInicial = 1,
        NumeroFinal = 1,
        Justificativa = "Numeracao reservada e nao utilizada por falha na emissao",
        CertificadoBase64 = "QUJD",
        CertificadoSenha = "senha",
        Ambiente = "homologacao",
        Uf = "MG",
    };

    private static IEnumerable<string> Erros(InutilizarNumeracaoRequest r) =>
        Validador.Validate(r).Errors.Select(e => e.ErrorMessage);

    [Fact]
    public void Pedido_valido_e_aceito()
    {
        Validador.Validate(Valido()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Numero_unico_e_aceito()
    {
        var r = Valido();
        r.NumeroInicial = 7;
        r.NumeroFinal = 7;

        // Inutilizar um numero so e o caso comum — foi o que sobrou da NF-e nº 1.
        Validador.Validate(r).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Faixa_invertida_e_recusada()
    {
        var r = Valido();
        r.NumeroInicial = 10;
        r.NumeroFinal = 5;

        Erros(r).Should().Contain("Numero final deve ser maior ou igual ao inicial");
    }

    [Fact]
    public void Justificativa_curta_demais_e_recusada()
    {
        var r = Valido();
        r.Justificativa = "erro";

        Erros(r).Should().Contain(e => e.Contains("no minimo 15"));
    }

    [Fact]
    public void Justificativa_longa_demais_e_recusada()
    {
        var r = Valido();
        r.Justificativa = new string('a', 256);

        Erros(r).Should().Contain(e => e.Contains("no maximo 255"));
    }

    [Theory]
    [InlineData(55)]
    [InlineData(65)]
    public void Modelos_emitidos_pelo_sistema_sao_aceitos(int modelo)
    {
        var r = Valido();
        r.Modelo = modelo;

        Validador.Validate(r).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Modelo_fora_dos_dois_e_recusado()
    {
        var r = Valido();
        r.Modelo = 57;

        Erros(r).Should().Contain("Modelo deve ser 55 (NF-e) ou 65 (NFC-e)");
    }

    [Fact]
    public void Cnpj_invalido_e_recusado()
    {
        var r = Valido();
        r.Cnpj = "11111111111111";

        Erros(r).Should().Contain("CNPJ do emitente e invalido");
    }

    [Fact]
    public void Uf_e_obrigatoria_porque_nao_ha_chave_de_onde_deduzi_la()
    {
        var r = Valido();
        r.Uf = "";

        // A faixa nunca virou documento: nao existe chave de acesso com o
        // codigo da UF, entao ela precisa vir no pedido.
        Erros(r).Should().Contain("UF do emitente e obrigatoria");
    }

    [Fact]
    public void Ano_de_dois_digitos_e_recusado()
    {
        var r = Valido();
        r.Ano = 26;

        Erros(r).Should().Contain("Ano da numeracao deve ter 4 digitos");
    }
}
