using FiscalService.Application.DTOs;
using FiscalService.Application.UseCases.EmitirNfce;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;

namespace FiscalService.Application.Mappers;

public static class NfceMapper
{
    public static Nfce ToDomain(EmitirNfceRequest request)
    {
        var ambiente = ToAmbiente(request.Ambiente);
        var nfce = new Nfce(request.Serie, request.Numero, ambiente);

        foreach (var item in request.Itens)
        {
            nfce.AdicionarItem(ToItem(item));
        }

        foreach (var pagamento in request.Pagamentos)
        {
            nfce.AdicionarPagamento(PagamentoMapper.ToDomain(pagamento));
        }

        return nfce;
    }

    public static NfceItem ToItem(ItemNfceDto dto)
    {
        return new NfceItem(
            dto.NumeroItem,
            dto.CodigoProduto,
            dto.Descricao,
            dto.Ncm,
            dto.Cfop,
            dto.UnidadeComercial,
            dto.Quantidade,
            dto.ValorUnitario,
            dto.Origem,
            dto.Csosn
        )
        {
            Cest = dto.Cest,
            Gtin = dto.Gtin
        };
    }

    public static Ambiente ToAmbiente(string ambiente)
    {
        return ambiente.ToLowerInvariant() switch
        {
            "producao" or "produção" => Ambiente.Producao,
            "homologacao" or "homologação" => Ambiente.Homologacao,
            _ => throw new ArgumentException($"Ambiente invalido: {ambiente}")
        };
    }
}
