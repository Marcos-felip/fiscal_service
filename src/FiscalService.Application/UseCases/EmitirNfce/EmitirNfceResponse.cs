using FiscalService.Application.DTOs;

namespace FiscalService.Application.UseCases.EmitirNfce;

public class EmitirNfceResponse
{
    public bool Sucesso { get; set; }
    public string? ChaveAcesso { get; set; }
    public string? Protocolo { get; set; }
    public string? XmlAutorizadoBase64 { get; set; }
    public string? DanfeBase64 { get; set; }
    public string? QrCode { get; set; }
    public RejeicaoDto? Rejeicao { get; set; }
}
