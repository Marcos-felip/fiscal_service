namespace FiscalService.Application.UseCases.ConsultarNfce;

public class ConsultarNfceResponse
{
    public bool Sucesso { get; set; }
    public string? Status { get; set; }
    public string? Protocolo { get; set; }
    public string? XmlConsultaBase64 { get; set; }
    public string? MensagemErro { get; set; }
}
