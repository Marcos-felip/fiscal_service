using FiscalService.Domain.Common;

namespace FiscalService.Domain.ValueObjects;

public class Endereco : ValueObject
{
    public string Logradouro { get; }
    public string Numero { get; }
    public string? Complemento { get; }
    public string Bairro { get; }
    public string CodigoMunicipio { get; }
    public string Municipio { get; }
    public string Uf { get; }
    public string Cep { get; }

    public Endereco(string logradouro, string numero, string? complemento, string bairro,
        string codigoMunicipio, string municipio, string uf, string cep)
    {
        Logradouro = logradouro;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        CodigoMunicipio = codigoMunicipio;
        Municipio = municipio;
        Uf = uf;
        Cep = cep;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Logradouro;
        yield return Numero;
        yield return Bairro;
        yield return CodigoMunicipio;
        yield return Uf;
    }
}
