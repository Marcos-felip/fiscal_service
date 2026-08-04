using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using FiscalService.Domain.ValueObjects;
using MediatR;

namespace FiscalService.Application.UseCases.ConsultarNfce;

public class ConsultarNfceHandler : IRequestHandler<ConsultarNfceRequest, ConsultarNfceResponse>
{
    private readonly IFiscalEngine _fiscalEngine;

    public ConsultarNfceHandler(IFiscalEngine fiscalEngine)
    {
        _fiscalEngine = fiscalEngine;
    }

    public async Task<ConsultarNfceResponse> Handle(ConsultarNfceRequest request, CancellationToken cancellationToken)
    {
        var certificado = new Certificado(request.CertificadoBase64, request.CertificadoSenha);
        var ambiente = NfceMapper.ToAmbiente(request.Ambiente);

        var resultado = await _fiscalEngine.Consultar(request.ChaveAcesso, certificado, ambiente, cancellationToken);

        return new ConsultarNfceResponse
        {
            Sucesso = resultado.Sucesso,
            Status = resultado.Status,
            Protocolo = resultado.Protocolo,
            XmlConsultaBase64 = resultado.XmlConsulta != null ? Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(resultado.XmlConsulta)) : null,
            MensagemErro = resultado.Sucesso ? null : "Falha na consulta"
        };
    }
}
