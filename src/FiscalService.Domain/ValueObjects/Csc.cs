using FiscalService.Domain.Common;

namespace FiscalService.Domain.ValueObjects;

public class Csc : ValueObject
{
    public string Codigo { get; }
    public string IdCsc { get; }

    public Csc(string codigo, string idCsc)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("Codigo CSC nao pode ser vazio", nameof(codigo));

        if (string.IsNullOrWhiteSpace(idCsc))
            throw new ArgumentException("Id CSC nao pode ser vazio", nameof(idCsc));

        Codigo = codigo.Trim();
        IdCsc = idCsc.Trim();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Codigo;
        yield return IdCsc;
    }
}
