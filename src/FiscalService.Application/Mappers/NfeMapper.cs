using FiscalService.Application.DTOs;
using FiscalService.Application.UseCases.EmitirNfe;
using FiscalService.Domain.Entities;
using FiscalService.Domain.Enums;
using FiscalService.Domain.ValueObjects;

namespace FiscalService.Application.Mappers;

/// <summary>
/// Traducao do payload de NF-e para o dominio. O que e comum a NFC-e — emitente, item,
/// pagamento, ambiente, CRT — vem do <see cref="NfceMapper"/>, sem copia.
/// </summary>
public static class NfeMapper
{
    public static Nfe ToDomain(EmitirNfeRequest request)
    {
        var nfe = new Nfe(
            request.Serie,
            request.Numero,
            NfceMapper.ToAmbiente(request.Ambiente),
            NfceMapper.ToEmitente(request.Emitente),
            ToDestinatario(request.Destinatario),
            ToTipoOperacao(request.TipoOperacao),
            ToFinalidade(request.Finalidade),
            request.ConsumidorFinal,
            ToPresenca(request.Presenca),
            request.NaturezaOperacao,
            ToTransporte(request.Transporte),
            ToCobranca(request.Cobranca));

        foreach (var item in request.Itens)
            nfe.AdicionarItem(NfceMapper.ToItem(item));

        foreach (var pagamento in request.Pagamentos)
            nfe.AdicionarPagamento(PagamentoMapper.ToDomain(pagamento));

        return nfe;
    }

    public static DestinatarioNfe ToDestinatario(DestinatarioNfeDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return new DestinatarioNfe(
            new CpfCnpj(SomenteDigitos(dto.CpfCnpj)),
            dto.Nome,
            new Endereco(
                dto.Logradouro,
                dto.Numero,
                dto.Complemento,
                dto.Bairro,
                SomenteDigitos(dto.CodigoMunicipio),
                dto.Municipio,
                dto.Uf?.Trim().ToUpperInvariant() ?? string.Empty,
                SomenteDigitos(dto.Cep)),
            ToIndicadorIe(dto.IndicadorIe),
            dto.InscricaoEstadual,
            dto.Telefone,
            dto.Email);
    }

    // ---------------------------------------------------------------- enums, com Try* para os validators

    public static IndicadorIeDestinatario ToIndicadorIe(int valor)
        => TryToIndicadorIe(valor, out var indicador)
            ? indicador
            : throw new ArgumentException($"Indicador de IE invalido: {valor}. Use 1, 2 ou 9");

    public static bool TryToIndicadorIe(int valor, out IndicadorIeDestinatario indicador)
    {
        indicador = (IndicadorIeDestinatario)valor;
        return Enum.IsDefined(indicador);
    }

    public static TipoOperacao ToTipoOperacao(int valor)
        => TryToTipoOperacao(valor, out var tipo)
            ? tipo
            : throw new ArgumentException($"Tipo de operacao invalido: {valor}. Use 0 ou 1");

    public static bool TryToTipoOperacao(int valor, out TipoOperacao tipo)
    {
        tipo = (TipoOperacao)valor;
        return Enum.IsDefined(tipo);
    }

    public static FinalidadeNfe ToFinalidade(int valor)
        => TryToFinalidade(valor, out var finalidade)
            ? finalidade
            : throw new ArgumentException($"Finalidade invalida: {valor}. Use de 1 a 4");

    public static bool TryToFinalidade(int valor, out FinalidadeNfe finalidade)
    {
        finalidade = (FinalidadeNfe)valor;
        return Enum.IsDefined(finalidade);
    }

    public static PresencaComprador ToPresenca(int valor)
        => TryToPresenca(valor, out var presenca)
            ? presenca
            : throw new ArgumentException($"Indicador de presenca invalido: {valor}");

    public static bool TryToPresenca(int valor, out PresencaComprador presenca)
    {
        presenca = (PresencaComprador)valor;
        return Enum.IsDefined(presenca);
    }

    public static bool TryToModalidadeFrete(int valor, out ModalidadeFrete modalidade)
    {
        modalidade = (ModalidadeFrete)valor;
        return Enum.IsDefined(modalidade);
    }

    // ---------------------------------------------------------------- grupos opcionais

    /// <summary>Sem grupo de transporte, a nota declara "sem frete" — nao omite o grupo.</summary>
    public static Transporte ToTransporte(TransporteDto? dto)
    {
        if (dto is null)
            return Transporte.SemFrete();

        if (!TryToModalidadeFrete(dto.Modalidade, out var modalidade))
            throw new ArgumentException($"Modalidade de frete invalida: {dto.Modalidade}");

        return new Transporte(
            modalidade,
            dto.Transportadora is null
                ? null
                : new Transportadora(
                    new CpfCnpj(SomenteDigitos(dto.Transportadora.CpfCnpj)),
                    dto.Transportadora.Nome,
                    dto.Transportadora.InscricaoEstadual,
                    dto.Transportadora.Endereco,
                    dto.Transportadora.Municipio,
                    dto.Transportadora.Uf),
            dto.Veiculo is null
                ? null
                : new Veiculo(dto.Veiculo.Placa, dto.Veiculo.Uf, dto.Veiculo.Rntc),
            dto.Volumes?.Select(v => new Volume(
                v.Quantidade, v.Especie, v.Marca, v.Numeracao, v.PesoLiquido, v.PesoBruto)));
    }

    public static Cobranca? ToCobranca(CobrancaDto? dto)
    {
        if (dto is null)
            return null;

        return new Cobranca(
            dto.NumeroFatura,
            dto.ValorOriginal,
            dto.ValorDesconto,
            dto.ValorLiquido,
            dto.Duplicatas?.Select(d => new Duplicata(d.Numero, d.Vencimento, d.Valor)));
    }

    private static string SomenteDigitos(string? valor)
        => string.IsNullOrEmpty(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());
}
