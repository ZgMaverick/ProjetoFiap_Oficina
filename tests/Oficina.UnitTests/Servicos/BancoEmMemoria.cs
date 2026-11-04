using Microsoft.EntityFrameworkCore;
using Oficina.Application.Cadastros;
using Oficina.Application.OrdensDeServico;
using Oficina.Domain.Clientes;
using Oficina.Domain.Estoque;
using Oficina.Domain.OrdensDeServico;
using Oficina.Domain.Servicos;
using Oficina.Domain.Veiculos;
using Oficina.Infrastructure.Persistencia;

namespace Oficina.UnitTests.Servicos;

// Monta a camada de serviço com os repositórios reais sobre o banco em memória do EF.
// Cada teste cria a sua instância, com um banco vazio e só dele.
public sealed class BancoEmMemoria : IDisposable
{
    private readonly string _nome = $"teste-{Guid.NewGuid()}";

    public OficinaDbContext Contexto { get; }

    public BancoEmMemoria() => Contexto = NovoContexto();

    // Um contexto novo enxerga só o que foi de fato salvo, não o que está na memória do anterior.
    public OficinaDbContext NovoContexto()
        => new(new DbContextOptionsBuilder<OficinaDbContext>().UseInMemoryDatabase(_nome).Options);

    public OrdemServicoService OrdemServico() => new(
        new Repositorio<OrdemDeServico>(Contexto),
        new Repositorio<Cliente>(Contexto),
        new Repositorio<Veiculo>(Contexto),
        new Repositorio<Servico>(Contexto),
        new Repositorio<PecaInsumo>(Contexto),
        Contexto);

    public ClienteService Clientes() => new(new Repositorio<Cliente>(Contexto), Contexto);

    public PecaInsumoService Pecas() => new(new Repositorio<PecaInsumo>(Contexto), Contexto);

    public ServicoCatalogoService Catalogo() => new(new Repositorio<Servico>(Contexto), Contexto);

    public VeiculoService Veiculos() => new(new Repositorio<Veiculo>(Contexto), new Repositorio<Cliente>(Contexto), Contexto);

    public void Dispose() => Contexto.Dispose();
}
