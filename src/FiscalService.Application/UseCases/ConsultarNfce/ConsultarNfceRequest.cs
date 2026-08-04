using MediatR;

namespace FiscalService.Application.UseCases.ConsultarNfce;

public class ConsultarNfceRequest : IRequest<ConsultarNfceResponse>
{
    public string ChaveAcesso { get; set; } = string.Empty;
    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;
    public string Ambiente { get; set; } = string.Empty;
}
