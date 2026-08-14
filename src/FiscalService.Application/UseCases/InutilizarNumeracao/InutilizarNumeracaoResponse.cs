namespace FiscalService.Application.UseCases.InutilizarNumeracao;

/// <summary>
/// Retorno proprio, e nao o dos eventos: a inutilizacao nao tem chave de acesso
/// nem protocolo de evento. O que ela devolve e a situacao da <b>faixa</b>.
/// </summary>
public class InutilizarNumeracaoResponse
{
    public bool Sucesso { get; set; }

    /// <summary>Protocolo da inutilizacao homologada.</summary>
    public string? Protocolo { get; set; }

    public string? XmlInutilizacaoBase64 { get; set; }
    public string? MotivoRejeicao { get; set; }
}
