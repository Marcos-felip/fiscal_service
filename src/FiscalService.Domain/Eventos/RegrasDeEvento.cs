namespace FiscalService.Domain.Eventos;

/// <summary>
/// Limites dos eventos fiscais, como o layout os define.
///
/// Ficam no dominio, e nao espalhados nos validators, porque sao a mesma regra
/// citada em tres lugares: a validacao que recusa, a mensagem que explica e o
/// teste que fixa. Tres copias divergem.
/// </summary>
public static class RegrasDeEvento
{
    // ---- Carta de Correcao (110110) ----

    public const int CorrecaoTamanhoMinimo = 15;
    public const int CorrecaoTamanhoMaximo = 1000;

    /// <summary>
    /// Sequencia da CC-e. O maximo e o proprio limite de correcoes por nota.
    ///
    /// O motor confere apenas a <b>faixa</b>. Se a sequencia repete ou salta, e
    /// quantas correcoes a nota ja teve, sao perguntas sobre o historico do
    /// documento — e o motor nao persiste nada. Quem responde e o chamador.
    /// </summary>
    public const int SequenciaMinima = 1;
    public const int SequenciaMaxima = 20;

    /// <summary>
    /// Texto legal de condicao de uso da CC-e, do layout da NF-e.
    ///
    /// Vai no XML e volta no retorno para ser mostrado a quem confirma. E o que
    /// o layout oferece no lugar de uma validacao de conteudo: a CC-e e texto
    /// livre, e o que ela pode ou nao corrigir e responsabilidade do emitente.
    /// </summary>
    public const string CondicaoDeUso =
        "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do " +
        "Convenio S/N, de 15 de dezembro de 1970 e pode ser utilizada para " +
        "regularizacao de erro ocorrido na emissao de documento fiscal, desde " +
        "que o erro nao esteja relacionado com: I - as variaveis que determinam " +
        "o valor do imposto tais como: base de calculo, aliquota, diferenca de " +
        "preco, quantidade, valor da operacao ou da prestacao; II - a correcao " +
        "de dados cadastrais que implique mudanca do remetente ou do " +
        "destinatario; III - a data de emissao ou de saida.";

    // ---- Inutilizacao ----

    public const int JustificativaTamanhoMinimo = 15;
    public const int JustificativaTamanhoMaximo = 255;

    /// <summary>Numeracao aceita pelo layout, igual a da emissao.</summary>
    public const int NumeroMinimo = 1;
    public const int NumeroMaximo = 999_999_999;

    public const int SerieMinima = 0;
    public const int SerieMaxima = 999;
}
