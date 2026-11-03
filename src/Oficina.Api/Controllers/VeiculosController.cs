using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Cadastros;

namespace Oficina.Api.Controllers;

[ApiController]
[Route("api/veiculos")]
[Authorize]
public class VeiculosController(VeiculoService service) : ControllerBase
{
    [HttpGet]
    public Task<List<VeiculoResponse>> Listar(CancellationToken ct) => service.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<VeiculoResponse> Obter(Guid id, CancellationToken ct) => service.ObterAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<VeiculoResponse>> Criar(VeiculoRequest req, CancellationToken ct)
    {
        var veiculo = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = veiculo.Id }, veiculo);
    }

    [HttpPut("{id:guid}")]
    public Task<VeiculoResponse> Atualizar(Guid id, VeiculoRequest req, CancellationToken ct)
        => service.AtualizarAsync(id, req, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
