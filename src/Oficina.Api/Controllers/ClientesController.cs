using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.Cadastros;

namespace Oficina.Api.Controllers;

// O controller só recebe a requisição e chama a camada de serviço.
[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController(ClienteService service) : ControllerBase
{
    [HttpGet]
    public Task<List<ClienteResponse>> Listar(CancellationToken ct) => service.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    public Task<ClienteResponse> Obter(Guid id, CancellationToken ct) => service.ObterAsync(id, ct);

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Criar(ClienteRequest req, CancellationToken ct)
    {
        var cliente = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:guid}")]
    public Task<ClienteResponse> Atualizar(Guid id, ClienteRequest req, CancellationToken ct)
        => service.AtualizarAsync(id, req, ct);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await service.RemoverAsync(id, ct);
        return NoContent();
    }
}
