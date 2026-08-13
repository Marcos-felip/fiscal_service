using FiscalService.Application.DTOs;
using MediatR;

namespace FiscalService.Application.UseCases.EmitirNfe;

/// <summary>
/// Requisicao de emissao de NF-e modelo 55.
///
/// Nao reaproveita <c>EmitirNfceRequest</c> de proposito: as duas divergem no que e
/// obrigatorio. Aqui o destinatario existe sempre e o CSC nao existe nunca — juntar as duas
/// num DTO so transformaria toda regra de obrigatoriedade em <c>if</c> de modelo.
/// </summary>
public class EmitirNfeRequest : IRequest<EmitirNfeResponse>
{
    public EmitenteDto Emitente { get; set; } = null!;

    /// <summary>Obrigatorio no modelo 55, ao contrario da NFC-e.</summary>
    public DestinatarioNfeDto Destinatario { get; set; } = null!;

    public List<ItemNfceDto> Itens { get; set; } = new();
    public List<PagamentoDto> Pagamentos { get; set; } = new();
    public decimal ValorTotal { get; set; }

    public string CertificadoBase64 { get; set; } = string.Empty;
    public string CertificadoSenha { get; set; } = string.Empty;

    public int Serie { get; set; }
    public int Numero { get; set; }
    public string Ambiente { get; set; } = string.Empty;

    /// <summary>Texto livre. Sem valor, vale "VENDA DE MERCADORIA".</summary>
    public string? NaturezaOperacao { get; set; }

    /// <summary>0 entrada, 1 saida. O recorte atual aceita apenas saida.</summary>
    public int TipoOperacao { get; set; } = 1;

    /// <summary>1 normal, 2 complementar, 3 ajuste, 4 devolucao. O recorte aceita apenas normal.</summary>
    public int Finalidade { get; set; } = 1;

    /// <summary>
    /// <c>indFinal</c>: venda para consumo (true) ou para revenda (false). Quem sabe o destino
    /// da mercadoria e quem lancou a venda — o motor nao tem como deduzir.
    /// </summary>
    public bool ConsumidorFinal { get; set; }

    /// <summary><c>indPres</c>: 0 nao se aplica, 1 presencial, 2 internet, 9 outros.</summary>
    public int Presenca { get; set; } = 1;

    public TransporteDto? Transporte { get; set; }
    public CobrancaDto? Cobranca { get; set; }

    /// <summary>
    /// Existe apenas para ser recusado. O CSC e da NFC-e; aceitar em silencio esconderia
    /// um erro de contrato do chamador.
    /// </summary>
    public string? CodigoCsc { get; set; }

    /// <inheritdoc cref="CodigoCsc"/>
    public string? IdCsc { get; set; }
}
