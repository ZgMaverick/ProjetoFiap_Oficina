using Oficina.Domain.Comum;

namespace Oficina.Domain.Veiculos;

public class Veiculo
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid ClienteId { get; private set; }
    public Placa Placa { get; private set; } = null!;
    public string Marca { get; private set; } = "";
    public string Modelo { get; private set; } = "";
    public int Ano { get; private set; }

    private Veiculo() { }

    public Veiculo(Guid clienteId, Placa placa, string marca, string modelo, int ano)
    {
        ClienteId = clienteId;
        Placa = placa;
        Atualizar(marca, modelo, ano);
    }

    public void Atualizar(string marca, string modelo, int ano)
    {
        if (string.IsNullOrWhiteSpace(marca) || string.IsNullOrWhiteSpace(modelo))
            throw new DomainException("Marca e modelo são obrigatórios.");
        if (ano < 1886 || ano > DateTime.UtcNow.Year + 1) //No caso EXTREMAMENTE RARO de alguem aparecer com um Benz Patent-Motorwagen
            throw new DomainException("Ano do veículo inválido.");

        Marca = marca.Trim();
        Modelo = modelo.Trim();
        Ano = ano;
    }
}
