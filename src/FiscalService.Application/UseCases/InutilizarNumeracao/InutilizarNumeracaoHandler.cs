using System.Text;
using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using FiscalService.Domain.ValueObjects;
using MediatR;

namespace FiscalService.Application.UseCases.InutilizarNumeracao;

public class InutilizarNumeracaoHandler
    : IRequestHandler<InutilizarNumeracaoRequest, InutilizarNumeracaoResponse>
{
    private readonly IFiscalEngine _fiscalEngine;

    public InutilizarNumeracaoHandler(IFiscalEngine fiscalEngine)
    {
        _fiscalEngine = fiscalEngine;
    }

    public async Task<InutilizarNumeracaoResponse> Handle(
        InutilizarNumeracaoRequest request,
        CancellationToken cancellationToken)
    {
        var certificado = new Certificado(
            request.CertificadoBase64,
            request.CertificadoSenha);

        var resultado = await _fiscalEngine.Inutilizar(
            new InutilizacaoPedido(
                request.Cnpj,
                request.Ano,
                request.Modelo,
                request.Serie,
                request.NumeroInicial,
                request.NumeroFinal,
                request.Justificativa,
                request.Uf),
            certificado,
            NfceMapper.ToAmbiente(request.Ambiente),
            cancellationToken);

        return new InutilizarNumeracaoResponse
        {
            Sucesso = resultado.Sucesso,
            Protocolo = resultado.Protocolo,
            XmlInutilizacaoBase64 = resultado.XmlInutilizacao is null
                ? null
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(resultado.XmlInutilizacao)),
            MotivoRejeicao = resultado.MotivoRejeicao,
        };
    }
}
