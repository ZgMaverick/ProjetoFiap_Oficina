using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Cadastros;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("api/servicos")]
[Authorize]
public class ServicosController(ServicoCatalogoService service) : ControllerBase
{
    [HttpGet]
    public Task<List<ServicoResponse>> Listar(CancellationToken ct) => service.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<ServicoResponse> Obter(Guid id, CancellationToken ct) => service.ObterAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ServicoResponse>> Criar(ServicoRequest req, CancellationToken ct)
    {
        var servico = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = servico.Id }, servico);
    }

    [HttpPut("{id:guid}")]
    public Task<ServicoResponse> Atualizar(Guid id, ServicoRequest req, CancellationToken ct)
        => service.AtualizarAsync(id, req, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
