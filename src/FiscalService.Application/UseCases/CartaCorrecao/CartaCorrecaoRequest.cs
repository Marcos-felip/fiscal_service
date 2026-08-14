using MediatR;

namespace FiscalService.Application.UseCases.CartaCorrecao;

/// <summary>
/// Carta de Correcao Eletronica (evento 110110).
///
/// O motor confere o que e local — tamanho do texto e faixa da sequencia. Se a
/// sequencia repete ou salta, e quantas correcoes a nota ja teve, sao perguntas
/// sobre o historico do documento: quem responde e o chamador, que tem os
/// eventos gravados.
/// </summary>
public class CartaCorrecaoRequest : IRequest<CartaCorrecaoResponse>
{
    public string ChaveAcesso { get; set; } = string.Empty;

    /// <summary>Texto da correcao, de 15 a 1000 caracteres.</summary>
    public string Correcao { get; set; } = string.Empty;

    /// <summary>
    /// Sequencia do evento para esta nota, de 1 a 20. A primeira correcao e 1.
    /// </summary>
    public int SequenciaEvento { get; set; } = 1;

    /// <summary>CNPJ ou CPF do autor do evento — o emitente da nota.</summary>
    public string CpfCnpj { get; set; } = string.Empty;

    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
}
