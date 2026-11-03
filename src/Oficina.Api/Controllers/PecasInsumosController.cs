using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Cadastros;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("api/pecas-insumos")]
[Authorize]
public class PecasInsumosController(PecaInsumoService service) : ControllerBase
{
    [HttpGet]
    public Task<List<PecaInsumoResponse>> Listar(CancellationToken ct) => service.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<PecaInsumoResponse> Obter(Guid id, CancellationToken ct) => service.ObterAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<PecaInsumoResponse>> Criar(PecaInsumoRequest req, CancellationToken ct)
    {
        var peca = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = peca.Id }, peca);
    }

    [HttpPut("{id:guid}")]
    public Task<PecaInsumoResponse> Atualizar(Guid id, PecaInsumoRequest req, CancellationToken ct)
        => service.AtualizarAsync(id, req, ct);

    // Entrada de mercadoria: aumenta o saldo físico.
    [HttpPost("{id:guid}/entrada")]
    public Task<PecaInsumoResponse> RegistrarEntrada(Guid id, EntradaEstoqueRequest req, CancellationToken ct)
        => service.RegistrarEntradaAsync(id, req, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
