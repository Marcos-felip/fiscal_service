namespace FiscalService.Application.Validators;

/// <summary>
/// Regras de formato/consistencia reutilizadas pelos validators. Ficam aqui, e nao no
/// Domain, porque sao validacao de payload de entrada e nao invariante de dominio.
/// </summary>
public static class ValidacoesFiscais
{
    public static readonly string[] UnidadesFederativas =
    {
        "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO", "MA", "MT", "MS", "MG",
        "PA", "PB", "PR", "PE", "PI", "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
    };

    /// <summary>
    /// CSOSN aceitos pelo adapter (Simples Nacional). Os demais exigem base de calculo
    /// e aliquota, que nao vem no payload.
    /// </summary>
    public static readonly string[] CsosnSuportados = { "102", "103", "300", "400", "500" };

    /// <summary>CST de ICMS aceitos pelo adapter (Regime Normal), pelo mesmo motivo.</summary>
    public static readonly string[] CstIcmsSuportados = { "40", "41", "50" };

    public static string SomenteDigitos(string? valor)
    {
        return string.IsNullOrEmpty(valor)
            ? string.Empty
            : new string(valor.Where(char.IsDigit).ToArray());
    }

    public static bool UfValida(string? uf)
    {
        return !string.IsNullOrWhiteSpace(uf)
               && UnidadesFederativas.Contains(uf.Trim().ToUpperInvariant());
    }

    /// <summary>Codigo IBGE do municipio: 7 digitos, sendo os 2 primeiros o codigo da UF.</summary>
    public static bool CodigoMunicipioValido(string? codigo)
    {
        var digitos = SomenteDigitos(codigo);
        return digitos.Length == 7 && digitos != new string('0', 7);
    }

    public static bool CepValido(string? cep) => SomenteDigitos(cep).Length == 8;

    public static bool NcmValido(string? ncm)
    {
        var digitos = SomenteDigitos(ncm);
        // 8 digitos no caso geral; 2 e tolerado para itens sem NCM detalhado.
        return digitos.Length == 8 || digitos.Length == 2;
    }

    public static bool CfopValido(string? cfop)
    {
        var digitos = SomenteDigitos(cfop);
        // NFC-e e sempre saida dentro do estado, entao CFOP comeca com 5.
        return digitos.Length == 4 && digitos[0] == '5';
    }

    /// <summary>GTIN valido tem 8, 12, 13 ou 14 digitos. Vazio significa "SEM GTIN".</summary>
    public static bool GtinValido(string? gtin)
    {
        if (string.IsNullOrWhiteSpace(gtin))
            return true;

        var digitos = SomenteDigitos(gtin);
        return digitos.Length == gtin.Trim().Length
               && digitos.Length is 8 or 12 or 13 or 14
               && DigitoGtinCorreto(digitos);
    }

    private static bool DigitoGtinCorreto(string gtin)
    {
        var soma = 0;
        var peso = 3;

        for (var i = gtin.Length - 2; i >= 0; i--)
        {
            soma += (gtin[i] - '0') * peso;
            peso = peso == 3 ? 1 : 3;
        }

        var verificador = (10 - soma % 10) % 10;
        return verificador == gtin[^1] - '0';
    }

    public static bool ChaveAcessoValida(string? chave)
    {
        var digitos = SomenteDigitos(chave);

        if (digitos.Length != 44 || digitos.Length != (chave?.Trim().Length ?? 0))
            return false;

        var soma = 0;
        var peso = 2;

        for (var i = 42; i >= 0; i--)
        {
            soma += (digitos[i] - '0') * peso;
            peso = peso == 9 ? 2 : peso + 1;
        }

        var resto = soma % 11;
        var verificador = resto is 0 or 1 ? 0 : 11 - resto;

        return verificador == digitos[43] - '0';
    }

    public static bool CpfOuCnpjValido(string? valor)
    {
        var digitos = SomenteDigitos(valor);

        return digitos.Length switch
        {
            11 => CpfValido(digitos),
            14 => CnpjValido(digitos),
            _ => false
        };
    }

    public static bool CnpjValido(string? valor)
    {
        var d = SomenteDigitos(valor);

        if (d.Length != 14 || d.All(c => c == d[0]))
            return false;

        var pesos1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var pesos2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        return d[12] - '0' == DigitoModulo11(d, pesos1)
               && d[13] - '0' == DigitoModulo11(d, pesos2);
    }

    public static bool CpfValido(string? valor)
    {
        var d = SomenteDigitos(valor);

        if (d.Length != 11 || d.All(c => c == d[0]))
            return false;

        var pesos1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var pesos2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        return d[9] - '0' == DigitoModulo11(d, pesos1)
               && d[10] - '0' == DigitoModulo11(d, pesos2);
    }

    private static int DigitoModulo11(string digitos, int[] pesos)
    {
        var soma = 0;

        for (var i = 0; i < pesos.Length; i++)
            soma += (digitos[i] - '0') * pesos[i];

        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }
}
