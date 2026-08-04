using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Domain.Entities;

public class Nfce : BaseEntity
{
    public const string NaturezaOperacaoPadrao = "VENDA AO CONSUMIDOR";

    public int Serie { get; private set; }
    public int Numero { get; private set; }
    public Ambiente Ambiente { get; private set; }
    public Emitente Emitente { get; private set; }
    public Destinatario? Destinatario { get; private set; }
    public string NaturezaOperacao { get; private set; }
    public DateTimeOffset DataEmissao { get; private set; }
    public NfceStatus Status { get; private set; } = NfceStatus.Pendente;
    public string? ChaveAcesso { get; private set; }
    public string? Protocolo { get; private set; }
    public DateTime? DataAutorizacao { get; private set; }

    private readonly List<NfceItem> _itens = new();
    public IReadOnlyCollection<NfceItem> Itens => _itens.AsReadOnly();

    private readonly List<Pagamento> _pagamentos = new();
    public IReadOnlyCollection<Pagamento> Pagamentos => _pagamentos.AsReadOnly();

    public decimal ValorTotal => _itens.Sum(i => i.ValorTotal);

    public Nfce(int serie, int numero, Ambiente ambiente, Emitente emitente,
        Destinatario? destinatario = null, string? naturezaOperacao = null, DateTimeOffset? dataEmissao = null)
    {
        Serie = serie;
        Numero = numero;
        Ambiente = ambiente;
        Emitente = emitente ?? throw new ArgumentNullException(nameof(emitente));
        Destinatario = destinatario;
        NaturezaOperacao = string.IsNullOrWhiteSpace(naturezaOperacao) ? NaturezaOperacaoPadrao : naturezaOperacao;
        DataEmissao = dataEmissao ?? DateTimeOffset.Now;
    }

    public void AdicionarItem(NfceItem item)
    {
        _itens.Add(item);
    }

    public void AdicionarPagamento(Pagamento pagamento)
    {
        _pagamentos.Add(pagamento);
    }

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

    public void Rejeitar(string motivo)
    {
        Status = NfceStatus.Rejeitado;
        UpdatedAt = DateTime.UtcNow;
    }
}
