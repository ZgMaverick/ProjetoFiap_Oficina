using Oficina.Application.Abstracoes;
using Oficina.Domain.Clientes;
using Oficina.Domain.Comum;
using Oficina.Domain.Veiculos;

namespace Oficina.Application.Cadastros;

public class VeiculoService(IRepositorio<Veiculo> veiculos, IRepositorio<Cliente> clientes, IUnidadeDeTrabalho uow)
{
    public async Task<List<VeiculoResponse>> ListarAsync(CancellationToken ct = default)
        => (await veiculos.ListarAsync(null, ct)).Select(VeiculoResponse.De).ToList();

    public async Task<VeiculoResponse> ObterAsync(Guid id, CancellationToken ct = default)
        => VeiculoResponse.De(await CarregarAsync(id, ct));

    public async Task<VeiculoResponse> CriarAsync(VeiculoRequest req, CancellationToken ct = default)
    {
        if (await clientes.ObterPorIdAsync(req.ClienteId, ct) is null)
            throw new NaoEncontradoException("Cliente não encontrado.");

        var placa = new Placa(req.Placa);
        if ((await veiculos.ListarAsync(v => v.Placa == placa, ct)).Count > 0)
            throw new DomainException("Já existe um veículo com esta placa.");

        var veiculo = new Veiculo(req.ClienteId, placa, req.Marca, req.Modelo, req.Ano);
        await veiculos.AdicionarAsync(veiculo, ct);
        await uow.SalvarAsync(ct);
        return VeiculoResponse.De(veiculo);
    }

    public async Task<VeiculoResponse> AtualizarAsync(Guid id, VeiculoRequest req, CancellationToken ct = default)
    {
        var veiculo = await CarregarAsync(id, ct);
        veiculo.Atualizar(req.Marca, req.Modelo, req.Ano); // a placa não muda
        await uow.SalvarAsync(ct);
        return VeiculoResponse.De(veiculo);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        veiculos.Remover(await CarregarAsync(id, ct));
        await uow.SalvarAsync(ct);
    }

    private async Task<Veiculo> CarregarAsync(Guid id, CancellationToken ct)
        => await veiculos.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Veículo não encontrado.");
}
