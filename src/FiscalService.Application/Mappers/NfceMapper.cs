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
        return TryToAmbiente(ambiente, out var valor)
            ? valor
            : throw new ArgumentException($"Ambiente invalido: {ambiente}");
    }

    /// <summary>Usado tambem pelos validators, para nao duplicar a lista de valores aceitos.</summary>
    public static bool TryToAmbiente(string? ambiente, out Ambiente valor)
    {
        switch (ambiente?.Trim().ToLowerInvariant())
        {
            case "producao":
            case "produção":
            case "1":
                valor = Ambiente.Producao;
                return true;
            case "homologacao":
            case "homologação":
            case "2":
                valor = Ambiente.Homologacao;
                return true;
            default:
                valor = default;
                return false;
        }
    }

    public static Crt ToCrt(string crt)
    {
        return TryToCrt(crt, out var valor)
            ? valor
            : throw new ArgumentException($"CRT invalido: {crt}");
    }

    public static bool TryToCrt(string? crt, out Crt valor)
    {
        switch (crt?.Trim().ToLowerInvariant())
        {
            case "1":
            case "simplesnacional":
            case "simples_nacional":
                valor = Crt.SimplesNacional;
                return true;
            case "2":
            case "simplesnacionalexcessosublimite":
            case "simples_nacional_excesso":
                valor = Crt.SimplesNacionalExcessoSublimite;
                return true;
            case "3":
            case "regimenormal":
            case "regime_normal":
                valor = Crt.RegimeNormal;
                return true;
            case "4":
            case "simplesnacionalmei":
            case "simples_nacional_mei":
            case "mei":
                valor = Crt.SimplesNacionalMei;
                return true;
            default:
                valor = default;
                return false;
        }
    }

    private static string SomenteDigitos(string? valor)
    {
        return string.IsNullOrEmpty(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());
    }
}
