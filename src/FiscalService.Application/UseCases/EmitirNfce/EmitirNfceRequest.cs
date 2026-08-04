using FiscalService.Application.DTOs;
using MediatR;

namespace FiscalService.Application.UseCases.EmitirNfce;

public class EmitirNfceRequest : IRequest<EmitirNfceResponse>
{
    public EmitenteDto Emitente { get; set; } = null!;
    public DestinatarioDto Destinatario { get; set; } = null!;
    public List<ItemNfceDto> Itens { get; set; } = new();
    public List<PagamentoDto> Pagamentos { get; set; } = new();
    public decimal ValorTotal { get; set; }
    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;
    public string CodigoCsc { get; set; } = string.Empty;
    public string IdCsc { get; set; } = string.Empty;
    public int Serie { get; set; }
    public int Numero { get; set; }
    public string Ambiente { get; set; } = string.Empty;
}
