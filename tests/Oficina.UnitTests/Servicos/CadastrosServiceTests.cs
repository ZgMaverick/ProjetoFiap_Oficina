using Oficina.Application.Abstracoes;
using Oficina.Application.Cadastros;
using Oficina.Domain.Comum;

namespace Oficina.UnitTests.Servicos;

public class CadastrosServiceTests : IDisposable
{
    private readonly BancoEmMemoria _banco = new();

    public void Dispose() => _banco.Dispose();

    [Fact]
    public async Task Cliente_criado_e_gravado_sem_mascara()
    {
        var cliente = await _banco.Clientes().CriarAsync(new ClienteRequest("Ana", "529.982.247-25", null, null));

        var gravado = await _banco.Clientes().ObterAsync(cliente.Id);
        Assert.Equal("52998224725", gravado.Documento);
    }

    [Fact]
    public async Task Nao_cadastra_dois_clientes_com_o_mesmo_documento()
    {
        await _banco.Clientes().CriarAsync(new ClienteRequest("Ana", "52998224725", null, null));

        await Assert.ThrowsAsync<DomainException>(() =>
            _banco.Clientes().CriarAsync(new ClienteRequest("Outra Ana", "529.982.247-25", null, null)));
    }

    [Fact]
    public async Task Atualizar_e_remover_cliente()
    {
        var cliente = await _banco.Clientes().CriarAsync(new ClienteRequest("Ana", "52998224725", null, null));

        var atualizado = await _banco.Clientes().AtualizarAsync(cliente.Id,
            new ClienteRequest("Ana Souza", "52998224725", "48999990000", null));
        Assert.Equal("Ana Souza", atualizado.Nome);

        await _banco.Clientes().RemoverAsync(cliente.Id);
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _banco.Clientes().ObterAsync(cliente.Id));
    }

    [Fact]
    public async Task Entrada_de_estoque_aumenta_o_saldo_fisico()
    {
        var peca = await _banco.Pecas().CriarAsync(new PecaInsumoRequest("oleo-5w30", "Óleo 5W30", "L", 38m));

        var atualizada = await _banco.Pecas().RegistrarEntradaAsync(peca.Id, new EntradaEstoqueRequest(20));

        Assert.Equal("OLEO-5W30", atualizada.Codigo);
        Assert.Equal(20, atualizada.SaldoFisico);
        Assert.Equal(20, atualizada.SaldoDisponivel);
    }

    [Fact]
    public async Task Nao_cadastra_duas_pecas_com_o_mesmo_codigo()
    {
        await _banco.Pecas().CriarAsync(new PecaInsumoRequest("FLT-001", "Filtro", "un", 45m));

        await Assert.ThrowsAsync<DomainException>(() =>
            _banco.Pecas().CriarAsync(new PecaInsumoRequest("flt-001", "Outro filtro", "un", 40m)));
    }

    [Fact]
    public async Task Catalogo_de_servicos_lista_o_que_foi_criado()
    {
        await _banco.Catalogo().CriarAsync(new ServicoRequest("Alinhamento", 80m, 40));
        await _banco.Catalogo().CriarAsync(new ServicoRequest("Balanceamento", 60m, 30));

        Assert.Equal(2, (await _banco.Catalogo().ListarAsync()).Count);
    }

    [Fact]
    public async Task Atualizar_e_remover_servico_do_catalogo()
    {
        var servico = await _banco.Catalogo().CriarAsync(new ServicoRequest("Alinhamento", 80m, 40));

        var atualizado = await _banco.Catalogo().AtualizarAsync(servico.Id, new ServicoRequest("Alinhamento 3D", 120m, 50));
        Assert.Equal("ALINHAMENTO 3D", (await _banco.Catalogo().ObterAsync(servico.Id)).Nome);
        Assert.Equal(120m, atualizado.Preco);

        await _banco.Catalogo().RemoverAsync(servico.Id);
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _banco.Catalogo().ObterAsync(servico.Id));
    }

    [Fact]
    public async Task Atualizar_listar_e_remover_peca()
    {
        var peca = await _banco.Pecas().CriarAsync(new PecaInsumoRequest("FLT-001", "Filtro", "un", 45m));

        await _banco.Pecas().AtualizarAsync(peca.Id, new PecaInsumoRequest("FLT-001", "Filtro de ar", "un", 50m));
        Assert.Equal("Filtro de ar", (await _banco.Pecas().ObterAsync(peca.Id)).Nome);
        Assert.Single(await _banco.Pecas().ListarAsync());

        await _banco.Pecas().RemoverAsync(peca.Id);
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _banco.Pecas().ObterAsync(peca.Id));
    }

    [Fact]
    public async Task Nao_remove_peca_com_reserva_ativa()
    {
        var peca = await _banco.Pecas().CriarAsync(new PecaInsumoRequest("FLT-001", "Filtro", "un", 45m));
        await _banco.Pecas().RegistrarEntradaAsync(peca.Id, new EntradaEstoqueRequest(5));
        var entidade = await _banco.Contexto.PecasInsumos.FindAsync(peca.Id);
        entidade!.Reservar(2);
        await _banco.Contexto.SaveChangesAsync();

        await Assert.ThrowsAsync<DomainException>(() => _banco.Pecas().RemoverAsync(peca.Id));
    }

    [Fact]
    public async Task Peca_sem_codigo_e_rejeitada()
    {
        await Assert.ThrowsAsync<DomainException>(() =>
            _banco.Pecas().CriarAsync(new PecaInsumoRequest(null!, "Filtro", "un", 45m)));
    }

    [Fact]
    public async Task Ciclo_completo_de_veiculo()
    {
        var cliente = await _banco.Clientes().CriarAsync(new ClienteRequest("Ana", "52998224725", null, null));

        var veiculo = await _banco.Veiculos().CriarAsync(new VeiculoRequest(cliente.Id, "abc1d23", "Fiat", "Uno", 2015));
        Assert.Equal("ABC1D23", veiculo.Placa);

        var atualizado = await _banco.Veiculos().AtualizarAsync(veiculo.Id, new VeiculoRequest(cliente.Id, "ABC1D23", "Fiat", "Uno Way", 2016));
        Assert.Equal("Uno Way", atualizado.Modelo);
        Assert.Single(await _banco.Veiculos().ListarAsync());

        await _banco.Veiculos().RemoverAsync(veiculo.Id);
        await Assert.ThrowsAsync<NaoEncontradoException>(() => _banco.Veiculos().ObterAsync(veiculo.Id));
    }

    [Fact]
    public async Task Veiculo_exige_cliente_existente()
    {
        await Assert.ThrowsAsync<NaoEncontradoException>(() =>
            _banco.Veiculos().CriarAsync(new VeiculoRequest(Guid.NewGuid(), "ABC1D23", "Fiat", "Uno", 2015)));
    }

    [Fact]
    public async Task Nao_cadastra_dois_veiculos_com_a_mesma_placa()
    {
        var cliente = await _banco.Clientes().CriarAsync(new ClienteRequest("Ana", "52998224725", null, null));
        await _banco.Veiculos().CriarAsync(new VeiculoRequest(cliente.Id, "ABC1D23", "Fiat", "Uno", 2015));

        await Assert.ThrowsAsync<DomainException>(() =>
            _banco.Veiculos().CriarAsync(new VeiculoRequest(cliente.Id, "ABC1D23", "VW", "Gol", 2018)));
    }

    [Theory]
    [InlineData("", "Uno", 2015)]
    [InlineData("Fiat", " ", 2015)]
    [InlineData("Fiat", "Uno", 1800)]
    [InlineData("Fiat", "Uno", 3000)]
    public async Task Veiculo_com_dados_invalidos_e_rejeitado(string marca, string modelo, int ano)
    {
        var cliente = await _banco.Clientes().CriarAsync(new ClienteRequest("Ana", "52998224725", null, null));

        await Assert.ThrowsAsync<DomainException>(() =>
            _banco.Veiculos().CriarAsync(new VeiculoRequest(cliente.Id, "ABC1D23", marca, modelo, ano)));
    }
}
