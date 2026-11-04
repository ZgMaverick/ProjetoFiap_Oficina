using System.Text.RegularExpressions;

namespace Oficina.Domain.Comum;

// Objeto de valor: placa no formato antigo (ABC1234) ou Mercosul (ABC1D23).
public sealed partial record Placa
{
    public string Valor { get; }

    public Placa(string valor)
    {
        var placa = (valor ?? "").Replace("-", "").Replace(" ", "").ToUpperInvariant();

        if (!FormatoValido().IsMatch(placa))
            throw new DomainException("Placa inválida. Use o formato ABC1234 ou ABC1D23.");

        Valor = placa;
    }

    [GeneratedRegex("^[A-Z]{3}[0-9][A-Z0-9][0-9]{2}$")]
    private static partial Regex FormatoValido();

    public override string ToString() => Valor;
}
