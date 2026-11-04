using Oficina.Application.Abstracoes;
using Oficina.Domain.Clientes;
using Oficina.Domain.Comum;
using Oficina.Domain.Estoque;
using Oficina.Domain.OrdensDeServico;
using Oficina.Domain.Servicos;
using Oficina.Domain.Veiculos;

namespace Oficina.Application.OrdensDeServico;

// Casos de uso da Ordem de Serviço. Cada método carrega o que precisa,
// chama o domínio e salva tudo de uma vez (uma transação).
public class OrdemServicoService(
    IRepositorio<OrdemDeServico> ordens,
    IRepositorio<Cliente> clientes,
    IRepositorio<Veiculo> veiculos,
    IRepositorio<Servico> servicos,
    IRepositorio<PecaInsumo> pecas,
    IUnidadeDeTrabalho unidadeDeTrabalho)
{
    public async Task<OrdemServicoResponse> CriarAsync(CriarOrdemServicoRequest req, CancellationToken ct = default)
    {
        // 1. Identifica o cliente pelo CPF/CNPJ (ou cadastra, se for novo)
        var documento = new Documento(req.DocumentoCliente);
        var cliente = (await clientes.ListarAsync(c => c.Documento == documento, ct)).FirstOrDefault();
        if (cliente is null)
        {
            cliente = new Cliente(req.NomeCliente, documento, req.TelefoneCliente, req.EmailCliente);
            await clientes.AdicionarAsync(cliente, ct);
        }

        // 2. Identifica o veículo pela placa (ou cadastra)
        var placa = new Placa(req.Placa);
        var veiculo = (await veiculos.ListarAsync(v => v.Placa == placa, ct)).FirstOrDefault();
        if (veiculo is null)
        {
            veiculo = new Veiculo(cliente.Id, placa, req.Marca, req.Modelo, req.Ano);
            await veiculos.AdicionarAsync(veiculo, ct);
        }

        // 3. Abre a OS com status Recebida
        var os = new OrdemDeServico(cliente.Id, veiculo.Id, req.RelatoCliente);
        await ordens.AdicionarAsync(os, ct);

        await unidadeDeTrabalho.SalvarAsync(ct);
        return OrdemServicoResponse.De(os);
    }

    public async Task<OrdemServicoResponse> ObterAsync(Guid id, CancellationToken ct = default)
        => OrdemServicoResponse.De(await CarregarAsync(id, ct));

    public async Task<List<OrdemServicoResponse>> ListarAsync(StatusOrdemServico? status, CancellationToken ct = default)
    {
        var lista = status is null
            ? await ordens.ListarAsync(null, ct)
            : await ordens.ListarAsync(o => o.Status == status, ct);
        return lista.OrderByDescending(o => o.CriadaEm).Select(OrdemServicoResponse.De).ToList();
    }

    public Task<OrdemServicoResponse> IniciarDiagnosticoAsync(Guid id, CancellationToken ct = default)
        => AlterarAsync(id, os => os.IniciarDiagnostico(), ct);

    public Task<OrdemServicoResponse> RegistrarDiagnosticoAsync(Guid id, string diagnostico, CancellationToken ct = default)
        => AlterarAsync(id, os => os.RegistrarDiagnostico(diagnostico), ct);

    public async Task<OrdemServicoResponse> AdicionarServicoAsync(Guid id, Guid servicoId, CancellationToken ct = default)
    {
        var servico = await servicos.ObterPorIdAsync(servicoId, ct)
            ?? throw new NaoEncontradoException("Serviço não encontrado.");
        return await AlterarAsync(id, os => os.AdicionarServico(servico), ct);
    }

    // Inclui a peça no orçamento e já reserva no estoque, na mesma transação.
    public async Task<OrdemServicoResponse> AdicionarPecaInsumoAsync(Guid id, AdicionarPecaRequest req, CancellationToken ct = default)
    {
        var peca = await pecas.ObterPorIdAsync(req.PecaInsumoId, ct)
            ?? throw new NaoEncontradoException("Peça ou insumo não encontrado.");

        return await AlterarAsync(id, os =>
        {
            peca.Reservar(req.Quantidade);          // falha aqui se não houver saldo
            os.AdicionarPecaInsumo(peca, req.Quantidade);
        }, ct);
    }

    public Task<OrdemServicoResponse> EnviarOrcamentoAsync(Guid id, CancellationToken ct = default)
        => AlterarAsync(id, os => os.EnviarOrcamento(), ct);

    public Task<OrdemServicoResponse> AprovarOrcamentoAsync(Guid id, CancellationToken ct = default)
        => AlterarAsync(id, os => os.AprovarOrcamento(), ct);

    // Recusa: cancela a OS e devolve as peças reservadas.
    public async Task<OrdemServicoResponse> RecusarOrcamentoAsync(Guid id, CancellationToken ct = default)
    {
        var os = await CarregarAsync(id, ct);
        os.RecusarOrcamento();
        await MovimentarPecasAsync(os, (peca, qtd) => peca.DevolverReserva(qtd), ct);
        await unidadeDeTrabalho.SalvarAsync(ct);
        return OrdemServicoResponse.De(os);
    }

    public async Task<OrdemServicoResponse> CancelarAsync(Guid id, CancellationToken ct = default)
    {
        var os = await CarregarAsync(id, ct);
        os.Cancelar();
        await MovimentarPecasAsync(os, (peca, qtd) => peca.DevolverReserva(qtd), ct);
        await unidadeDeTrabalho.SalvarAsync(ct);
        return OrdemServicoResponse.De(os);
    }

    // Finaliza: dá baixa no estoque de todas as peças usadas.
    public async Task<OrdemServicoResponse> FinalizarAsync(Guid id, CancellationToken ct = default)
    {
        var os = await CarregarAsync(id, ct);
        os.Finalizar();
        await MovimentarPecasAsync(os, (peca, qtd) => peca.DarBaixa(qtd), ct);
        await unidadeDeTrabalho.SalvarAsync(ct);
        return OrdemServicoResponse.De(os);
    }

    public Task<OrdemServicoResponse> EntregarAsync(Guid id, CancellationToken ct = default)
        => AlterarAsync(id, os => os.Entregar(), ct);

    // Consulta do cliente: só vê a OS se informar o próprio CPF/CNPJ.
    public async Task<OrdemServicoResponse> ConsultarPeloClienteAsync(Guid id, string documentoCliente, CancellationToken ct = default)
    {
        var os = await CarregarAsync(id, ct);
        var cliente = await clientes.ObterPorIdAsync(os.ClienteId, ct);
        var documento = new Documento(documentoCliente);

        if (cliente is null || cliente.Documento != documento)
            throw new NaoEncontradoException("Ordem de serviço não encontrada.");

        return OrdemServicoResponse.De(os);
    }

    public async Task<TempoMedioResponse> TempoMedioDeExecucaoAsync(CancellationToken ct = default)
    {
        var concluidas = await ordens.ListarAsync(
            o => o.Status == StatusOrdemServico.Finalizada || o.Status == StatusOrdemServico.Entregue, ct);

        var tempos = concluidas
            .Select(o => o.TempoDeExecucao())
            .Where(t => t is not null)
            .Select(t => t!.Value.TotalMinutes)
            .ToList();

        return new TempoMedioResponse(tempos.Count, tempos.Count == 0 ? 0 : Math.Round(tempos.Average(), 1));
    }

    private async Task<OrdemDeServico> CarregarAsync(Guid id, CancellationToken ct)
        => await ordens.ObterPorIdAsync(id, ct)
           ?? throw new NaoEncontradoException("Ordem de serviço não encontrada.");

    private async Task<OrdemServicoResponse> AlterarAsync(Guid id, Action<OrdemDeServico> acao, CancellationToken ct)
    {
        var os = await CarregarAsync(id, ct);
        acao(os);
        await unidadeDeTrabalho.SalvarAsync(ct);
        return OrdemServicoResponse.De(os);
    }

    private async Task MovimentarPecasAsync(OrdemDeServico os, Action<PecaInsumo, decimal> movimento, CancellationToken ct)
    {
        foreach (var item in os.ItensDePecas())
        {
            var peca = await pecas.ObterPorIdAsync(item.ReferenciaId, ct)
                ?? throw new NaoEncontradoException($"Peça {item.Descricao} não encontrada.");
            movimento(peca, item.Quantidade);
        }
    }
}
