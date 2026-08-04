using FiscalService.Domain.Common;
using FiscalService.Domain.Enums;

namespace FiscalService.Domain.Entities;

public class Pagamento : BaseEntity
{
    public TipoPagamento Tipo { get; private set; }
    public decimal Valor { get; private set; }

    public Pagamento(TipoPagamento tipo, decimal valor)
    {
        Tipo = tipo;
        Valor = valor;
    }
}
