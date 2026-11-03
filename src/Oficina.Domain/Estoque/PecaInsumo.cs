using Oficina.Domain.Comum;

namespace Oficina.Domain.Estoque;

// Peça ou insumo com controle de estoque.
// Saldo disponível = saldo físico - saldo reservado para OS.
public class PecaInsumo
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Codigo { get; private set; } = "";
    public string Nome { get; private set; } = "";
    public string Unidade { get; private set; } = "un";
    public decimal Preco { get; private set; }
    public decimal SaldoFisico { get; private set; }
    public decimal SaldoReservado { get; private set; }
    public decimal SaldoDisponivel => SaldoFisico - SaldoReservado;

    // Controle de concorrência: impede que duas OS reservem a mesma última peça.
    public uint Versao { get; private set; }

    private PecaInsumo() { }

    public PecaInsumo(string codigo, string nome, string unidade, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new DomainException("O código da peça é obrigatório.");

        Codigo = codigo.Trim().ToUpperInvariant();
        Atualizar(nome, unidade, preco);
    }

    public void Atualizar(string nome, string unidade, decimal preco)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da peça é obrigatório.");
        if (preco < 0)
            throw new DomainException("O preço não pode ser negativo.");

        Nome = nome.Trim();
        Unidade = string.IsNullOrWhiteSpace(unidade) ? "un" : unidade.Trim();
        Preco = preco;
    }

    public void RegistrarEntrada(decimal quantidade)
    {
        ValidarQuantidade(quantidade);
        SaldoFisico += quantidade;
    }

    public void Reservar(decimal quantidade)
    {
        ValidarQuantidade(quantidade);
        if (quantidade > SaldoDisponivel)
            throw new DomainException(
                $"Saldo insuficiente de {Nome}: disponível {SaldoDisponivel} {Unidade}, pedido {quantidade}.");

        SaldoReservado += quantidade;
    }

    public void DevolverReserva(decimal quantidade)
    {
        ValidarQuantidade(quantidade);
        if (quantidade > SaldoReservado)
            throw new DomainException("Não é possível devolver mais do que está reservado.");

        SaldoReservado -= quantidade;
    }

    // A baixa só acontece sobre o que estava reservado para a OS.
    public void DarBaixa(decimal quantidade)
    {
        ValidarQuantidade(quantidade);
        if (quantidade > SaldoReservado)
            throw new DomainException("A baixa só pode ser feita sobre quantidade reservada.");

        SaldoReservado -= quantidade;
        SaldoFisico -= quantidade;
    }

    private static void ValidarQuantidade(decimal quantidade)
    {
        if (quantidade <= 0)
            throw new DomainException("A quantidade deve ser maior que zero.");
    }
}
