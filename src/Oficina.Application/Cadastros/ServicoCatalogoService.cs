using Oficina.Application.Abstracoes;
using Oficina.Domain.Servicos;

namespace Oficina.Application.Cadastros;

// Cadastro dos serviços oferecidos pela oficina (troca de óleo, alinhamento...).
public class ServicoCatalogoService(IRepositorio<Servico> servicos, IUnidadeDeTrabalho uow)
{
    public async Task<List<ServicoResponse>> ListarAsync(CancellationToken ct = default)
        => (await servicos.ListarAsync(null, ct)).Select(ServicoResponse.De).ToList();

    public async Task<ServicoResponse> ObterAsync(Guid id, CancellationToken ct = default)
        => ServicoResponse.De(await CarregarAsync(id, ct));

    public async Task<ServicoResponse> CriarAsync(ServicoRequest req, CancellationToken ct = default)
    {
        var servico = new Servico(req.Nome, req.Preco, req.TempoEstimadoMinutos);
        await servicos.AdicionarAsync(servico, ct);
        await uow.SalvarAsync(ct);
        return ServicoResponse.De(servico);
    }

    public async Task<ServicoResponse> AtualizarAsync(Guid id, ServicoRequest req, CancellationToken ct = default)
    {
        var servico = await CarregarAsync(id, ct);
        servico.Atualizar(req.Nome, req.Preco, req.TempoEstimadoMinutos);
        await uow.SalvarAsync(ct);
        return ServicoResponse.De(servico);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        servicos.Remover(await CarregarAsync(id, ct));
        await uow.SalvarAsync(ct);
    }

    private async Task<Servico> CarregarAsync(Guid id, CancellationToken ct)
        => await servicos.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Serviço não encontrado.");
}
