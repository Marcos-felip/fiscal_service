using System.Text;
using FiscalService.Application.Interfaces;
using FiscalService.Application.Mappers;
using FiscalService.Domain.Eventos;
using FiscalService.Domain.ValueObjects;
using MediatR;

namespace FiscalService.Application.UseCases.CartaCorrecao;

public class CartaCorrecaoHandler
    : IRequestHandler<CartaCorrecaoRequest, CartaCorrecaoResponse>
{
    private readonly IFiscalEngine _fiscalEngine;

    public CartaCorrecaoHandler(IFiscalEngine fiscalEngine)
    {
        _fiscalEngine = fiscalEngine;
    }

    public async Task<CartaCorrecaoResponse> Handle(
        CartaCorrecaoRequest request,
        CancellationToken cancellationToken)
    {
        var certificado = new Certificado(
            request.CertificadoBase64,
            request.CertificadoSenha);

        var resultado = await _fiscalEngine.CartaCorrecao(
            request.ChaveAcesso,
            request.Correcao,
            request.SequenciaEvento,
            request.CpfCnpj,
            certificado,
            NfceMapper.ToAmbiente(request.Ambiente),
            cancellationToken);

        return new CartaCorrecaoResponse
        {
            Sucesso = resultado.Sucesso,
            Protocolo = resultado.Protocolo,
            XmlEventoBase64 = resultado.XmlEvento is null
                ? null
                : Convert.ToBase64String(Encoding.UTF8.GetBytes(resultado.XmlEvento)),
            MotivoRejeicao = resultado.MotivoRejeicao,
            // Volta mesmo na recusa: quem confirma a correcao precisa ler a
            // condicao de uso antes, nao depois de dar certo.
            CondicaoDeUso = RegrasDeEvento.CondicaoDeUso,
        };
    }
}
