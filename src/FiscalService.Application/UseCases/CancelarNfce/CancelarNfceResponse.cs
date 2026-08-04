namespace FiscalService.Application.UseCases.CancelarNfce;

public class CancelarNfceResponse
{
    public bool Sucesso { get; set; }
    public string? Protocolo { get; set; }
    public string? XmlCancelamentoBase64 { get; set; }
    public string? MotivoRejeicao { get; set; }
}
