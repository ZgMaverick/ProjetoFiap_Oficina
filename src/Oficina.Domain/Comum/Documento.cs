using Oficina.Domain.Comum;

namespace Oficina.Domain.Comum;

// Objeto de valor: CPF ou CNPJ já validado. Só guarda os dígitos.
public sealed record Documento
{
    public string Valor { get; }
    public bool EhCnpj => Valor.Length == 14;

    public Documento(string valor)
    {
        var digitos = new string((valor ?? "").Where(char.IsDigit).ToArray());

        var valido = digitos.Length switch
        {
            11 => CpfValido(digitos),
            14 => CnpjValido(digitos),
            _ => false
        };

        if (!valido)
            throw new DomainException("CPF ou CNPJ inválido.");

        Valor = digitos;
    }

    private static bool CpfValido(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false; // 111.111.111-11 etc.

        int[] pesos1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesos2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];
        return Digito(cpf[..9], pesos1) == cpf[9] - '0'
            && Digito(cpf[..10], pesos2) == cpf[10] - '0';
    }

    private static bool CnpjValido(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;

        int[] pesos1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] pesos2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        return Digito(cnpj[..12], pesos1) == cnpj[12] - '0'
            && Digito(cnpj[..13], pesos2) == cnpj[13] - '0';
    }

    private static int Digito(string numeros, int[] pesos)
    {
        var soma = numeros.Select((c, i) => (c - '0') * pesos[i]).Sum();
        var resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    public override string ToString() => Valor;
}
