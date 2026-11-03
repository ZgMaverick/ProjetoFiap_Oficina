using Oficina.Domain.Comum;

namespace Oficina.Domain.Clientes;

public class Cliente
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = "";
    public Documento Documento { get; private set; } = null!;
    public string? Telefone { get; private set; }
    public string? Email { get; private set; }

    private Cliente() { } // usado pelo Entity Framework

    public Cliente(string nome, Documento documento, string? telefone, string? email)
    {
        Documento = documento;
        Atualizar(nome, telefone, email);
    }

    public void Atualizar(string nome, string? telefone, string? email)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do cliente é obrigatório.");

        Nome = nome.Trim();
        Telefone = telefone;
        Email = email;
    }
}
