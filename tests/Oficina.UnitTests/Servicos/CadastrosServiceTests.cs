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
}
