using FiscalService.Application.DTOs;
using FiscalService.Application.UseCases.EmitirNfce;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Application.Mappers;

public static class NfceMapper
{
    public static Nfce ToDomain(EmitirNfceRequest request)
    {
        var ambiente = ToAmbiente(request.Ambiente);
        var nfce = new Nfce(
            request.Serie,
            request.Numero,
            ambiente,
            ToEmitente(request.Emitente),
            ToDestinatario(request.Destinatario));

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

    public static Emitente ToEmitente(EmitenteDto dto)
    {
        return new Emitente(
            new CpfCnpj(SomenteDigitos(dto.Cnpj)),
            dto.RazaoSocial,
            dto.NomeFantasia,
            dto.InscricaoEstadual,
            ToCrt(dto.Crt),
            new Endereco(
                dto.Logradouro,
                dto.Numero,
                dto.Complemento,
                dto.Bairro,
                dto.CodigoMunicipio,
                dto.Municipio,
                dto.Uf,
                SomenteDigitos(dto.Cep)),
            dto.Telefone,
            dto.Email
        );
    }

    public static Destinatario? ToDestinatario(DestinatarioDto? dto)
    {
        if (dto is null)
            return null;

        var documento = string.IsNullOrWhiteSpace(dto.CpfCnpj)
            ? null
            : new CpfCnpj(SomenteDigitos(dto.CpfCnpj));

        Endereco? endereco = null;
        if (!string.IsNullOrWhiteSpace(dto.Logradouro) && !string.IsNullOrWhiteSpace(dto.Uf))
        {
            endereco = new Endereco(
                dto.Logradouro!,
                dto.Numero ?? "S/N",
                dto.Complemento,
                dto.Bairro ?? string.Empty,
                dto.CodigoMunicipio ?? string.Empty,
                dto.Municipio ?? string.Empty,
                dto.Uf!,
                SomenteDigitos(dto.Cep));
        }

        var destinatario = new Destinatario(documento, dto.Nome, endereco);

        return destinatario.Identificado ? destinatario : null;
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
            "producao" or "produção" or "1" => Ambiente.Producao,
            "homologacao" or "homologação" or "2" => Ambiente.Homologacao,
            _ => throw new ArgumentException($"Ambiente invalido: {ambiente}")
        };
    }

    public static Crt ToCrt(string crt)
    {
        return crt.Trim().ToLowerInvariant() switch
        {
            "1" or "simplesnacional" or "simples_nacional" => Crt.SimplesNacional,
            "2" or "simplesnacionalexcessosublimite" or "simples_nacional_excesso" => Crt.SimplesNacionalExcessoSublimite,
            "3" or "regimenormal" or "regime_normal" => Crt.RegimeNormal,
            _ => throw new ArgumentException($"CRT invalido: {crt}")
        };
    }

    private static string SomenteDigitos(string? valor)
    {
        return string.IsNullOrEmpty(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());
    }
}
