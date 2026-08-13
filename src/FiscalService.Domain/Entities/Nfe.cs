using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Domain.Entities;

/// <summary>
/// NF-e modelo 55.
///
/// Nao herda da <see cref="Nfce"/> nem a reusa: o que as separa e justamente o que e
/// obrigatorio. Aqui o destinatario existe sempre, com endereco e indicador de IE; la ele
/// pode nao existir. O que e genuinamente comum — item, pagamento, emitente — e compartilhado
/// como tipo, nao como heranca.
/// </summary>
public class Nfe : BaseEntity
{
    public const string NaturezaOperacaoPadrao = "VENDA DE MERCADORIA";

    public int Serie { get; private set; }
    public int Numero { get; private set; }
    public Ambiente Ambiente { get; private set; }
    public Emitente Emitente { get; private set; }
    public DestinatarioNfe Destinatario { get; private set; }
    public string NaturezaOperacao { get; private set; }
    public TipoOperacao TipoOperacao { get; private set; }
    public FinalidadeNfe Finalidade { get; private set; }
    public bool ConsumidorFinal { get; private set; }
    public PresencaComprador Presenca { get; private set; }
    public DateTimeOffset DataEmissao { get; private set; }
    public Transporte Transporte { get; private set; }
    public Cobranca? Cobranca { get; private set; }

    public NfceStatus Status { get; private set; } = NfceStatus.Pendente;
    public string? ChaveAcesso { get; private set; }
    public string? Protocolo { get; private set; }
    public DateTime? DataAutorizacao { get; private set; }

    private readonly List<ItemFiscal> _itens = new();
    public IReadOnlyCollection<ItemFiscal> Itens => _itens.AsReadOnly();

    private readonly List<Pagamento> _pagamentos = new();
    public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos.AsReadOnly();

    public decimal ValorTotal => _itens.Sum(i => i.ValorTotal);

    public Nfe(
        int serie,
        int numero,
        Ambiente ambiente,
        Emitente emitente,
        DestinatarioNfe destinatario,
        TipoOperacao tipoOperacao,
        FinalidadeNfe finalidade,
        bool consumidorFinal,
        PresencaComprador presenca,
        string? naturezaOperacao = null,
        Transporte? transporte = null,
        Cobranca? cobranca = null,
        DateTimeOffset? dataEmissao = null)
    {
        Serie = serie;
        Numero = numero;
        Ambiente = ambiente;
        Emitente = emitente ?? throw new ArgumentNullException(nameof(emitente));
        Destinatario = destinatario ?? throw new ArgumentNullException(nameof(destinatario));
        TipoOperacao = tipoOperacao;
        Finalidade = finalidade;
        ConsumidorFinal = consumidorFinal;
        Presenca = presenca;
        NaturezaOperacao = string.IsNullOrWhiteSpace(naturezaOperacao)
            ? NaturezaOperacaoPadrao
            : naturezaOperacao;
        Transporte = transporte ?? Transporte.SemFrete();
        Cobranca = cobranca;
        DataEmissao = dataEmissao ?? DateTimeOffset.Now;
    }

    /// <summary>
    /// A operacao e interna quando emitente e destinatario estao na mesma UF. E o unico
    /// caso aceito no recorte atual, e tambem o que decide <c>idDest</c> no XML.
    /// </summary>
    public bool EhOperacaoInterna =>
        string.Equals(
            Emitente.Endereco.Uf?.Trim(),
            Destinatario.Endereco.Uf?.Trim(),
            StringComparison.OrdinalIgnoreCase);

    public void AdicionarItem(ItemFiscal item) => _itens.Add(item);

    public void AdicionarPagamento(Pagamento pagamento) => _pagamentos.Add(pagamento);

    public void Autorizar(string chaveAcesso, string protocolo)
    {
        ChaveAcesso = chaveAcesso;
        Protocolo = protocolo;
        DataAutorizacao = DateTime.UtcNow;
        Status = NfceStatus.Autorizado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancelar(string protocolo)
    {
        Protocolo = protocolo;
        Status = NfceStatus.Cancelado;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Rejeitar()
    {
        Status = NfceStatus.Rejeitado;
        UpdatedAt = DateTime.UtcNow;
    }
}
