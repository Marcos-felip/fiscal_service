using FiscalService.Application.DTOs;

namespace FiscalService.Application.UseCases.EmitirNfe;

public class EmitirNfeResponse
{
    public bool Sucesso { get; set; }
    public string? ChaveAcesso { get; set; }
    public string? Protocolo { get; set; }
    public string? XmlAutorizadoBase64 { get; set; }

    /// <summary>DANFE do modelo 55, em base64.</summary>
    public string? DanfeBase64 { get; set; }

    /// <summary>
    /// Tipo do conteudo em <see cref="DanfeBase64"/>. O DANFE da NF-e e HTML e o da NFC-e e
    /// PDF — quem grava e serve o arquivo precisa saber qual dos dois recebeu, em vez de
    /// assumir PDF como o resto do sistema faz hoje.
    /// </summary>
    public string? DanfeContentType { get; set; }

    public RejeicaoDto? Rejeicao { get; set; }
}
