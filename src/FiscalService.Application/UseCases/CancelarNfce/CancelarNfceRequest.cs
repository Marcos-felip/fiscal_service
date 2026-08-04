using MediatR;

namespace FiscalService.Application.UseCases.CancelarNfce;

public class CancelarNfceRequest : IRequest<CancelarNfceResponse>
{
    public string ChaveAcesso { get; set; } = string.Empty;
    public string ProtocoloAutorizacao { get; set; } = string.Empty;
    public string Justificativa { get; set; } = string.Empty;
    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
}
