using FiscalService.Application.Interfaces;
using FiscalService.Application.UseCases.CartaCorrecao;
using FiscalService.Domain.Enums;
using FiscalService.Domain.Eventos;
using FiscalService.Domain.ValueObjects;
using FluentAssertions;
using Moq;
using Xunit;

namespace FiscalService.UnitTests.Eventos;

/// <summary>
/// A CC-e e **texto livre**. O motor confere o que e local — tamanho do texto e
/// faixa da sequencia — e nao tenta deduzir se a correcao altera valores: seria
/// heuristica, que ou recusa correcao legitima ou aprova a ilegitima com ar de
/// validada.
///
/// O que o layout oferece no lugar e a condicao de uso, texto legal fixo que vai
/// no XML e volta no retorno para ser lido por quem confirma.
/// </summary>
public class CartaCorrecaoValidatorTests
{
    private static readonly CartaCorrecaoValidator Validador = new();

    /// <summary>Chave da NFC-e autorizada em homologacao em 13/08/2026.</summary>
    private const string CHAVE = "31260851720322000146650010000000011185782928";

    private static CartaCorrecaoRequest Valido() => new()
    {
        ChaveAcesso = CHAVE,
        Correcao = "Corrigir o nome do bairro do destinatario",
        SequenciaEvento = 1,
        CpfCnpj = "51720322000146",
        CertificadoBase64 = "QUJD",
        CertificadoSenha = "senha",
        Ambiente = "homologacao",
    };

    private static IEnumerable<string> Erros(CartaCorrecaoRequest r) =>
        Validador.Validate(r).Errors.Select(e => e.ErrorMessage);

    [Fact]
    public void Correcao_valida_e_aceita()
    {
        Validador.Validate(Valido()).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Texto_curto_demais_e_recusado()
    {
        var r = Valido();
        r.Correcao = "erro no nome";

        Erros(r).Should().Contain(e => e.Contains("no minimo 15"));
    }

    [Fact]
    public void Texto_longo_demais_e_recusado()
    {
        var r = Valido();
        r.Correcao = new string('a', RegrasDeEvento.CorrecaoTamanhoMaximo + 1);

        Erros(r).Should().Contain(e => e.Contains("no maximo 1000"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(21)]
    public void Sequencia_fora_da_faixa_legal_e_recusada(int sequencia)
    {
        var r = Valido();
        r.SequenciaEvento = sequencia;

        // A mensagem cita o limite de 20 porque e a razao da faixa, e sem isso
        // o "entre 1 e 20" pareceria arbitrario.
        Erros(r).Should().Contain(e => e.Contains("20 cartas de correcao por nota"));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(20)]
    public void Extremos_da_faixa_sao_aceitos(int sequencia)
    {
        var r = Valido();
        r.SequenciaEvento = sequencia;

        Validador.Validate(r).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Chave_de_acesso_invalida_e_recusada()
    {
        var r = Valido();
        r.ChaveAcesso = "31260851720322000146650010000000011185782929";

        Erros(r).Should().Contain("Chave de acesso invalida");
    }

    [Fact]
    public void Correcao_que_menciona_valor_nao_e_recusada_pelo_motor()
    {
        var r = Valido();
        r.Correcao = "Corrigir o valor unitario do item 1 para R$ 10,00";

        // O motor NAO julga o conteudo. Recusar aqui exigiria deduzir intencao
        // de texto livre; a restricao e legal e recai sobre o emitente, que le
        // a condicao de uso antes de confirmar.
        Validador.Validate(r).IsValid.Should().BeTrue();
    }
}

public class CartaCorrecaoHandlerTests
{
    [Fact]
    public async Task Condicao_de_uso_volta_no_sucesso()
    {
        var motor = new Mock<IFiscalEngine>();
        motor
            .Setup(m => m.CartaCorrecao(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<Certificado>(), It.IsAny<Ambiente>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventoResultado(true, "131260000000001", "<procEventoNFe/>", null));

        var resposta = await Handler(motor).Handle(Pedido(), CancellationToken.None);

        resposta.Sucesso.Should().BeTrue();
        resposta.CondicaoDeUso.Should().Be(RegrasDeEvento.CondicaoDeUso);
    }

    [Fact]
    public async Task Condicao_de_uso_volta_tambem_na_recusa()
    {
        var motor = new Mock<IFiscalEngine>();
        motor
            .Setup(m => m.CartaCorrecao(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<string>(), It.IsAny<Certificado>(), It.IsAny<Ambiente>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EventoResultado(false, null, null, "Rejeicao 573"));

        var resposta = await Handler(motor).Handle(Pedido(), CancellationToken.None);

        // Quem confirma a correcao precisa ler a condicao de uso antes, nao
        // depois de dar certo.
        resposta.Sucesso.Should().BeFalse();
        resposta.CondicaoDeUso.Should().Be(RegrasDeEvento.CondicaoDeUso);
    }

    [Fact]
    public void Condicao_de_uso_cita_o_que_a_cce_nao_corrige()
    {
        var texto = RegrasDeEvento.CondicaoDeUso;

        texto.Should().Contain("base de calculo");
        texto.Should().Contain("remetente ou do");
        texto.Should().Contain("data de emissao");
    }

    private static CartaCorrecaoHandler Handler(Mock<IFiscalEngine> motor) => new(motor.Object);

    private static CartaCorrecaoRequest Pedido() => new()
    {
        ChaveAcesso = "31260851720322000146650010000000011185782928",
        Correcao = "Corrigir o nome do bairro do destinatario",
        SequenciaEvento = 1,
        CpfCnpj = "51720322000146",
        CertificadoBase64 = "QUJD",
        CertificadoSenha = "senha",
        Ambiente = "homologacao",
    };
}
