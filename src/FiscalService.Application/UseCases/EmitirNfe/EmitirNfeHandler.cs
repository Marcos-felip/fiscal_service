using System.Text;
using FiscalService.Application.DTOs;
using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using FiscalService.Domain.ValueObjects;
using MediatR;

namespace FiscalService.Application.UseCases.EmitirNfe;

public class EmitirNfeHandler : IRequestHandler<EmitirNfeRequest, EmitirNfeResponse>
{
    private readonly IFiscalEngine _fiscalEngine;
    private readonly IDanfeNfeGenerator _danfeGenerator;

    public EmitirNfeHandler(IFiscalEngine fiscalEngine, IDanfeNfeGenerator danfeGenerator)
    {
        _fiscalEngine = fiscalEngine;
        _danfeGenerator = danfeGenerator;
    }

    public async Task<EmitirNfeResponse> Handle(EmitirNfeRequest request, CancellationToken cancellationToken)
    {
        var nfe = NfeMapper.ToDomain(request);
        var certificado = new Certificado(request.CertificadoBase64, request.CertificadoSenha);
        var ambiente = NfceMapper.ToAmbiente(request.Ambiente);

        var resultado = await _fiscalEngine.EmitirNfe(nfe, certificado, ambiente, cancellationToken);

        if (!resultado.Sucesso)
        {
            return new EmitirNfeResponse
            {
                Sucesso = false,
                Rejeicao = new RejeicaoDto(
                    resultado.CodigoRejeicao ?? "000",
                    resultado.MotivoRejeicao ?? "Erro desconhecido",
                    null)
            };
        }

        var xmlAutorizado = resultado.XmlAutorizado!;
        var danfe = _danfeGenerator.Gerar(xmlAutorizado);

        return new EmitirNfeResponse
        {
            Sucesso = true,
            ChaveAcesso = resultado.ChaveAcesso,
            Protocolo = resultado.Protocolo,
            XmlAutorizadoBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlAutorizado)),
            DanfeBase64 = Convert.ToBase64String(danfe.Conteudo),
            DanfeContentType = danfe.ContentType
        };
    }
}
