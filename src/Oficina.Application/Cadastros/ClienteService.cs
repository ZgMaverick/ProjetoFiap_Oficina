using Oficina.Application.Abstracoes;
using Oficina.Domain.Clientes;
using Oficina.Domain.Comum;

namespace Oficina.Application.Cadastros;

public class ClienteService(IRepositorio<Cliente> clientes, IUnidadeDeTrabalho uow)
{
    public async Task<List<ClienteResponse>> ListarAsync(CancellationToken ct = default)
        => (await clientes.ListarAsync(null, ct)).Select(ClienteResponse.De).ToList();

    public async Task<ClienteResponse> ObterAsync(Guid id, CancellationToken ct = default)
        => ClienteResponse.De(await CarregarAsync(id, ct));

    public async Task<ClienteResponse> CriarAsync(ClienteRequest req, CancellationToken ct = default)
    {
        var documento = new Documento(req.Documento);
        if ((await clientes.ListarAsync(c => c.Documento == documento, ct)).Count > 0)
            throw new DomainException("Já existe um cliente com este CPF/CNPJ.");

        var cliente = new Cliente(req.Nome, documento, req.Telefone, req.Email);
        await clientes.AdicionarAsync(cliente, ct);
        await uow.SalvarAsync(ct);
        return ClienteResponse.De(cliente);
    }

    public async Task<ClienteResponse> AtualizarAsync(Guid id, ClienteRequest req, CancellationToken ct = default)
    {
        var cliente = await CarregarAsync(id, ct);
        cliente.Atualizar(req.Nome, req.Telefone, req.Email); // o documento não muda
        await uow.SalvarAsync(ct);
        return ClienteResponse.De(cliente);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        clientes.Remover(await CarregarAsync(id, ct));
        await uow.SalvarAsync(ct);
    }

    private async Task<Cliente> CarregarAsync(Guid id, CancellationToken ct)
        => await clientes.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Cliente não encontrado.");
}
