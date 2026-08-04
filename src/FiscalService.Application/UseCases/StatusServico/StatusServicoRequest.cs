using MediatR;

namespace FiscalService.Application.UseCases.StatusServico;

public class StatusServicoRequest : IRequest<StatusServicoResponse>
{
    public string Ambiente { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;

    // Os web services da SEFAZ exigem certificado do contribuinte no handshake TLS,
    // inclusive no status-servico.
    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;
}
