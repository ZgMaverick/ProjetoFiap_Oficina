using Oficina.Application.Abstracoes;
using Oficina.Application.Cadastros;
using Oficina.Application.OrdensDeServico;
using Oficina.Domain.Comum;

namespace Oficina.UnitTests.Servicos;

public class OrdemServicoServiceTests : IDisposable
{
    private const string Cpf = "52998224725";
    private readonly BancoEmMemoria _banco = new();

    public void Dispose() => _banco.Dispose();

    private static CriarOrdemServicoRequest NovaOs(string documento = Cpf, string placa = "ABC1D23")
        => new(documento, "Ana Souza", null, null, placa, "Fiat", "Uno", 2015, "Barulho no motor");

    // Cria uma peça com saldo e uma OS em diagnóstico, prontas para receber itens.
    private async Task<(OrdemServicoResponse Os, PecaInsumoResponse Peca, ServicoResponse Servico)> CenarioAsync(decimal saldo = 5)
    {
        var peca = await _banco.Pecas().CriarAsync(new PecaInsumoRequest("FLT-001", "Filtro de óleo", "un", 45m));
        await _banco.Pecas().RegistrarEntradaAsync(peca.Id, new EntradaEstoqueRequest(saldo));
        var servico = await _banco.Catalogo().CriarAsync(new ServicoRequest("Troca de óleo", 80m, 30));

        var service = _banco.OrdemServico();
        var os = await service.CriarAsync(NovaOs());
        await service.IniciarDiagnosticoAsync(os.Id);
        await service.RegistrarDiagnosticoAsync(os.Id, "Óleo vencido");
        return (os, peca, servico);
    }

    [Fact]
    public async Task Criar_os_cadastra_cliente_e_veiculo_e_grava_no_banco()
    {
        var os = await _banco.OrdemServico().CriarAsync(NovaOs());

        using var outroContexto = _banco.NovoContexto();
        Assert.Single(outroContexto.Clientes);
        Assert.Single(outroContexto.Veiculos);
        Assert.Equal("Recebida", (await _banco.OrdemServico().ObterAsync(os.Id)).Status);
    }

    [Fact]
    public async Task Segunda_os_do_mesmo_cliente_reaproveita_o_cadastro()
    {
        var service = _banco.OrdemServico();
        var primeira = await service.CriarAsync(NovaOs(placa: "ABC1D23"));
        var segunda = await service.CriarAsync(NovaOs(placa: "XYZ9876"));

        Assert.Equal(primeira.ClienteId, segunda.ClienteId);
        Assert.Single(_banco.NovoContexto().Clientes);
    }

    [Fact]
    public async Task Adicionar_peca_reserva_no_estoque()
    {
        var (os, peca, _) = await CenarioAsync(saldo: 5);

        await _banco.OrdemServico().AdicionarPecaInsumoAsync(os.Id, new AdicionarPecaRequest(peca.Id, 2));

        var pecaGravada = await _banco.Pecas().ObterAsync(peca.Id);
        Assert.Equal(2, pecaGravada.SaldoReservado);
        Assert.Equal(3, pecaGravada.SaldoDisponivel);
    }

    [Fact]
    public async Task Sem_saldo_a_peca_nao_entra_na_os()
    {
        var (os, peca, _) = await CenarioAsync(saldo: 1);

        await Assert.ThrowsAsync<DomainException>(() =>
            _banco.OrdemServico().AdicionarPecaInsumoAsync(os.Id, new AdicionarPecaRequest(peca.Id, 2)));

        Assert.Empty((await _banco.OrdemServico().ObterAsync(os.Id)).Itens);
    }

    [Fact]
    public async Task Recusar_orcamento_cancela_e_devolve_a_reserva()
    {
        var (os, peca, servico) = await CenarioAsync();
        var service = _banco.OrdemServico();
        await service.AdicionarServicoAsync(os.Id, servico.Id);
        await service.AdicionarPecaInsumoAsync(os.Id, new AdicionarPecaRequest(peca.Id, 1));
        await service.EnviarOrcamentoAsync(os.Id);

        var resultado = await service.RecusarOrcamentoAsync(os.Id);

        Assert.Equal("Cancelada", resultado.Status);
        Assert.Equal(0, (await _banco.Pecas().ObterAsync(peca.Id)).SaldoReservado);
    }

    [Fact]
    public async Task Finalizar_da_baixa_no_estoque_e_entra_no_tempo_medio()
    {
        var (os, peca, servico) = await CenarioAsync(saldo: 5);
        var service = _banco.OrdemServico();
        await service.AdicionarServicoAsync(os.Id, servico.Id);
        await service.AdicionarPecaInsumoAsync(os.Id, new AdicionarPecaRequest(peca.Id, 1));
        var enviado = await service.EnviarOrcamentoAsync(os.Id);
        Assert.Equal(125m, enviado.ValorOrcamento);

        await service.AprovarOrcamentoAsync(os.Id);
        await service.FinalizarAsync(os.Id);
        var entregue = await service.EntregarAsync(os.Id);

        Assert.Equal("Entregue", entregue.Status);
        var pecaGravada = await _banco.Pecas().ObterAsync(peca.Id);
        Assert.Equal(4, pecaGravada.SaldoFisico);
        Assert.Equal(0, pecaGravada.SaldoReservado);
        Assert.Equal(1, (await service.TempoMedioDeExecucaoAsync()).OrdensFinalizadas);
    }

    [Fact]
    public async Task Cliente_nao_consulta_os_de_outro_cliente()
    {
        var os = await _banco.OrdemServico().CriarAsync(NovaOs());

        await Assert.ThrowsAsync<NaoEncontradoException>(() =>
            _banco.OrdemServico().ConsultarPeloClienteAsync(os.Id, "11144477735"));
    }

    [Fact]
    public async Task Os_inexistente_lanca_nao_encontrado()
        => await Assert.ThrowsAsync<NaoEncontradoException>(() => _banco.OrdemServico().ObterAsync(Guid.NewGuid()));
}
