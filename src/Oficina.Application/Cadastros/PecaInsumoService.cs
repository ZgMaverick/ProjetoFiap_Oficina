using Oficina.Application.Abstracoes;
using Oficina.Domain.Comum;
using Oficina.Domain.Estoque;

namespace Oficina.Application.Cadastros;

public class PecaInsumoService(IRepositorio<PecaInsumo> pecas, IUnidadeDeTrabalho uow)
{
    public async Task<List<PecaInsumoResponse>> ListarAsync(CancellationToken ct = default)
        => (await pecas.ListarAsync(null, ct)).Select(PecaInsumoResponse.De).ToList();

    public async Task<PecaInsumoResponse> ObterAsync(Guid id, CancellationToken ct = default)
        => PecaInsumoResponse.De(await CarregarAsync(id, ct));

    public async Task<PecaInsumoResponse> CriarAsync(PecaInsumoRequest req, CancellationToken ct = default)
    {
        var codigo = (req.Codigo ?? "").Trim().ToUpperInvariant();
        if ((await pecas.ListarAsync(p => p.Codigo == codigo, ct)).Count > 0)
            throw new DomainException("Já existe uma peça com este código.");

        var peca = new PecaInsumo(req.Codigo ?? "", req.Nome, req.Unidade, req.Preco);
        await pecas.AdicionarAsync(peca, ct);
        await uow.SalvarAsync(ct);
        return PecaInsumoResponse.De(peca);
    }

    public async Task<PecaInsumoResponse> AtualizarAsync(Guid id, PecaInsumoRequest req, CancellationToken ct = default)
    {
        var peca = await CarregarAsync(id, ct);
        peca.Atualizar(req.Nome, req.Unidade, req.Preco);
        await uow.SalvarAsync(ct);
        return PecaInsumoResponse.De(peca);
    }

    // Entrada de mercadoria: aumenta o saldo físico.
    public async Task<PecaInsumoResponse> RegistrarEntradaAsync(Guid id, EntradaEstoqueRequest req, CancellationToken ct = default)
    {
        var peca = await CarregarAsync(id, ct);
        peca.RegistrarEntrada(req.Quantidade);
        await uow.SalvarAsync(ct);
        return PecaInsumoResponse.De(peca);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var peca = await CarregarAsync(id, ct);
        if (peca.SaldoReservado > 0)
            throw new DomainException("Não é possível remover uma peça com reserva ativa.");

        pecas.Remover(peca);
        await uow.SalvarAsync(ct);
    }

    private async Task<PecaInsumo> CarregarAsync(Guid id, CancellationToken ct)
        => await pecas.ObterPorIdAsync(id, ct) ?? throw new NaoEncontradoException("Peça ou insumo não encontrado.");
}
