namespace FiscalService.Application.UseCases.CartaCorrecao;

public class CartaCorrecaoResponse
{
    public bool Sucesso { get; set; }
    public string? Protocolo { get; set; }
    public string? XmlEventoBase64 { get; set; }
    public string? MotivoRejeicao { get; set; }

    /// <summary>
    /// Texto legal de condicao de uso da CC-e.
    ///
    /// Volta sempre, inclusive na recusa, porque e o que o chamador mostra a
    /// quem confirma a correcao. E o instrumento que o layout oferece no lugar
    /// de uma validacao de conteudo: a CC-e e texto livre, e o que ela pode ou
    /// nao corrigir e responsabilidade do emitente.
    /// </summary>
    public string CondicaoDeUso { get; set; } = string.Empty;
}
