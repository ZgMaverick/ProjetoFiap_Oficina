using Oficina.Domain.Comum;

namespace Oficina.Domain.Servicos;

// Serviço do catálogo: troca de óleo, alinhamento etc.
public class Servico
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Nome { get; private set; } = "";
    public decimal Preco { get; private set; }
    public int TempoEstimadoMinutos { get; private set; }

    private Servico() { }

    public Servico(string nome, decimal preco, int tempoEstimadoMinutos)
        => Atualizar(nome, preco, tempoEstimadoMinutos);

    public void Atualizar(string nome, decimal preco, int tempoEstimadoMinutos)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome do serviço é obrigatório.");
        if (preco < 0)
            throw new DomainException("O preço não pode ser negativo.");

        Nome = nome.Trim();
        Preco = preco;
        TempoEstimadoMinutos = tempoEstimadoMinutos;
    }
}