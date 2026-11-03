using Oficina.Domain.Clientes;
using Oficina.Domain.Estoque;
using Oficina.Domain.Servicos;
using Oficina.Domain.Veiculos;

namespace Oficina.Application.Cadastros;

public record ClienteRequest(string Nome, string Documento, string? Telefone, string? Email);
public record ClienteResponse(Guid Id, string Nome, string Documento, string? Telefone, string? Email)
{
    public static ClienteResponse De(Cliente c) => new(c.Id, c.Nome, c.Documento.Valor, c.Telefone, c.Email);
}

public record VeiculoRequest(Guid ClienteId, string Placa, string Marca, string Modelo, int Ano);
public record VeiculoResponse(Guid Id, Guid ClienteId, string Placa, string Marca, string Modelo, int Ano)
{
    public static VeiculoResponse De(Veiculo v) => new(v.Id, v.ClienteId, v.Placa.Valor, v.Marca, v.Modelo, v.Ano);
}

public record ServicoRequest(string Nome, decimal Preco, int TempoEstimadoMinutos);
public record ServicoResponse(Guid Id, string Nome, decimal Preco, int TempoEstimadoMinutos)
{
    public static ServicoResponse De(Servico s) => new(s.Id, s.Nome, s.Preco, s.TempoEstimadoMinutos);
}

public record PecaInsumoRequest(string Codigo, string Nome, string Unidade, decimal Preco);
public record EntradaEstoqueRequest(decimal Quantidade);
public record PecaInsumoResponse(Guid Id, string Codigo, string Nome, string Unidade, decimal Preco,
    decimal SaldoFisico, decimal SaldoReservado, decimal SaldoDisponivel)
{
    public static PecaInsumoResponse De(PecaInsumo p) => new(p.Id, p.Codigo, p.Nome, p.Unidade, p.Preco,
        p.SaldoFisico, p.SaldoReservado, p.SaldoDisponivel);
}
