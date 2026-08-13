using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;

namespace FiscalService.Domain.ValueObjects;

/// <summary>
/// Destinatario da NF-e. Diferente do <see cref="Destinatario"/> da NFC-e, aqui documento,
/// nome, endereco e indicador de IE sao obrigatorios <b>por construcao</b> — um destinatario
/// de NF-e sem endereco nao deve conseguir existir como objeto.
///
/// A coerencia entre o indicador e a inscricao estadual tambem e invariante: contribuinte
/// sem IE, ou nao contribuinte com IE, sao contradicoes do proprio dado.
/// </summary>
public class DestinatarioNfe : ValueObject
{
    public CpfCnpj Documento { get; }
    public string Nome { get; }
    public Endereco Endereco { get; }
    public IndicadorIeDestinatario IndicadorIe { get; }
    public string? InscricaoEstadual { get; }
    public string? Telefone { get; }
    public string? Email { get; }

    public DestinatarioNfe(
        CpfCnpj documento,
        string nome,
        Endereco endereco,
        IndicadorIeDestinatario indicadorIe,
        string? inscricaoEstadual = null,
        string? telefone = null,
        string? email = null)
    {
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));
        Endereco = endereco ?? throw new ArgumentNullException(nameof(endereco));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do destinatario e obrigatorio", nameof(nome));

        if (!Enum.IsDefined(indicadorIe))
            throw new ArgumentException(
                $"Indicador de IE invalido: {(int)indicadorIe}. Use 1, 2 ou 9", nameof(indicadorIe));

        var ie = string.IsNullOrWhiteSpace(inscricaoEstadual) ? null : inscricaoEstadual.Trim();

        if (indicadorIe == IndicadorIeDestinatario.Contribuinte && ie is null)
            throw new ArgumentException(
                "Destinatario declarado contribuinte exige inscricao estadual", nameof(inscricaoEstadual));

        if (indicadorIe != IndicadorIeDestinatario.Contribuinte && ie is not null)
            throw new ArgumentException(
                $"Destinatario declarado {Descrever(indicadorIe)} nao pode ter inscricao estadual",
                nameof(inscricaoEstadual));

        Nome = nome.Trim();
        IndicadorIe = indicadorIe;
        InscricaoEstadual = ie;
        Telefone = telefone;
        Email = email;
    }

    public bool EhPessoaJuridica => Documento.EhCnpj;

    private static string Descrever(IndicadorIeDestinatario indicador) => indicador switch
    {
        IndicadorIeDestinatario.IsentoDeInscricao => "isento de inscricao estadual",
        IndicadorIeDestinatario.NaoContribuinte => "nao contribuinte",
        _ => "contribuinte"
    };

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Documento.Valor;
        yield return Nome;
        yield return (int)IndicadorIe;
        yield return InscricaoEstadual ?? string.Empty;
    }
}
