using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using MediatR;

namespace FiscalService.Application.UseCases.EmitirNfce;

public class EmitirNfceHandler : IRequestHandler<EmitirNfceRequest, EmitirNfceResponse>
{
    private readonly IFiscalEngine _fiscalEngine;
    private readonly IDanfeGenerator _danfeGenerator;

    public EmitirNfceHandler(IFiscalEngine fiscalEngine, IDanfeGenerator danfeGenerator)
    {
        _fiscalEngine = fiscalEngine;
        _danfeGenerator = danfeGenerator;
    }

    public async Task<EmitirNfceResponse> Handle(EmitirNfceRequest request, CancellationToken cancellationToken)
    {
        var nfce = NfceMapper.ToDomain(request);
        var certificado = new Domain.ValueObjects.Certificado(request.CertificadoBase64, request.CertificadoSenha);
        var csc = new Domain.ValueObjects.Csc(request.CodigoCsc, request.IdCsc);
        var ambiente = NfceMapper.ToAmbiente(request.Ambiente);

        var resultado = await _fiscalEngine.Emitir(nfce, certificado, csc, ambiente, cancellationToken);

        if (!resultado.Sucesso)
        {
            return new EmitirNfceResponse
            {
                Sucesso = false,
                Rejeicao = new DTOs.RejeicaoDto(
                    resultado.CodigoRejeicao ?? "000",
                    resultado.MotivoRejeicao ?? "Erro desconhecido",
                    null
                )
            };
        }

        var xmlAutorizado = resultado.XmlAutorizado!;
        var danfePdf = _danfeGenerator.GerarDanfe(xmlAutorizado);

        return new EmitirNfceResponse
        {
            Sucesso = true,
            ChaveAcesso = resultado.ChaveAcesso,
            Protocolo = resultado.Protocolo,
            XmlAutorizadoBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(xmlAutorizado)),
            DanfeBase64 = Convert.ToBase64String(danfePdf),
            QrCode = resultado.QrCode
        };
    }
}
