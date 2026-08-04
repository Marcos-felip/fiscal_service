using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using FiscalService.Domain.ValueObjects;
using MediatR;

namespace FiscalService.Application.UseCases.CancelarNfce;

public class CancelarNfceHandler : IRequestHandler<CancelarNfceRequest, CancelarNfceResponse>
{
    private readonly IFiscalEngine _fiscalEngine;

    public CancelarNfceHandler(IFiscalEngine fiscalEngine)
    {
        _fiscalEngine = fiscalEngine;
    }

    public async Task<CancelarNfceResponse> Handle(CancelarNfceRequest request, CancellationToken cancellationToken)
    {
        var certificado = new Certificado(request.CertificadoBase64, request.CertificadoSenha);
        var ambiente = NfceMapper.ToAmbiente(request.Ambiente);

        var resultado = await _fiscalEngine.Cancelar(
            request.ChaveAcesso,
            request.ProtocoloAutorizacao,
            request.Justificativa,
            certificado,
            ambiente,
            cancellationToken
        );

        return new CancelarNfceResponse
        {
            Sucesso = resultado.Sucesso,
            Protocolo = resultado.Protocolo,
            XmlCancelamentoBase64 = resultado.XmlCancelamento != null ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(resultado.XmlCancelamento)) : null,
            MotivoRejeicao = resultado.MotivoRejeicao
        };
    }
}
