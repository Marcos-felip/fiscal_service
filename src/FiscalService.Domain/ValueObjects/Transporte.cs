using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;

namespace FiscalService.Domain.ValueObjects;

/// <summary>
/// Grupo de transporte da NF-e. Sempre existe no XML — quando nao ha frete, existe declarando
/// justamente isso. Transportadora, veiculo e volumes sao opcionais dentro dele.
/// </summary>
public class Transporte : ValueObject
{
    public ModalidadeFrete Modalidade { get; }
    public Transportadora? Transportadora { get; }
    public Veiculo? Veiculo { get; }

    private readonly List<Volume> _volumes = new();
    public IReadOnlyCollection<Volume> Volumes => _volumes.AsReadOnly();

    public Transporte(
        ModalidadeFrete modalidade,
        Transportadora? transportadora = null,
        Veiculo? veiculo = null,
        IEnumerable<Volume>? volumes = null)
    {
        if (!Enum.IsDefined(modalidade))
            throw new ArgumentException($"Modalidade de frete invalida: {(int)modalidade}", nameof(modalidade));

        Modalidade = modalidade;
        Transportadora = transportadora;
        Veiculo = veiculo;

        if (volumes is not null)
            _volumes.AddRange(volumes);
    }

    /// <summary>Sem frete: o que a NFC-e sempre usou e o padrao de quem retira no balcao.</summary>
    public static Transporte SemFrete() => new(ModalidadeFrete.SemTransporte);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return (int)Modalidade;
        yield return Transportadora?.Documento.Valor ?? string.Empty;
        yield return Veiculo?.Placa ?? string.Empty;
        yield return _volumes.Count;
    }
}

public class Transportadora : ValueObject
{
    public CpfCnpj Documento { get; }
    public string Nome { get; }
    public string? InscricaoEstadual { get; }
    public string? EnderecoCompleto { get; }
    public string? Municipio { get; }
    public string? Uf { get; }

    public Transportadora(CpfCnpj documento, string nome, string? inscricaoEstadual = null,
        string? enderecoCompleto = null, string? municipio = null, string? uf = null)
    {
        Documento = documento ?? throw new ArgumentNullException(nameof(documento));

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da transportadora e obrigatorio", nameof(nome));

        Nome = nome.Trim();
        InscricaoEstadual = inscricaoEstadual;
        EnderecoCompleto = enderecoCompleto;
        Municipio = municipio;
        Uf = uf;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Documento.Valor;
        yield return Nome;
    }
}

public class Veiculo : ValueObject
{
    public string Placa { get; }
    public string Uf { get; }

    /// <summary>Registro Nacional de Transportador de Carga, quando o transportador tem.</summary>
    public string? Rntc { get; }

    public Veiculo(string placa, string uf, string? rntc = null)
    {
        if (string.IsNullOrWhiteSpace(placa))
            throw new ArgumentException("Placa do veiculo e obrigatoria", nameof(placa));

        if (string.IsNullOrWhiteSpace(uf))
            throw new ArgumentException("UF do veiculo e obrigatoria", nameof(uf));

        Placa = placa.Trim().ToUpperInvariant();
        Uf = uf.Trim().ToUpperInvariant();
        Rntc = rntc;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Placa;
        yield return Uf;
    }
}

public class Volume : ValueObject
{
    /// <summary>Quantidade de volumes. Inteira no layout: nao existe meia caixa.</summary>
    public int? Quantidade { get; }

    public string? Especie { get; }
    public string? Marca { get; }
    public string? Numeracao { get; }
    public decimal? PesoLiquido { get; }
    public decimal? PesoBruto { get; }

    public Volume(int? quantidade = null, string? especie = null, string? marca = null,
        string? numeracao = null, decimal? pesoLiquido = null, decimal? pesoBruto = null)
    {
        Quantidade = quantidade;
        Especie = especie;
        Marca = marca;
        Numeracao = numeracao;
        PesoLiquido = pesoLiquido;
        PesoBruto = pesoBruto;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Quantidade ?? 0;
        yield return Especie ?? string.Empty;
        yield return PesoBruto ?? 0m;
    }
}
