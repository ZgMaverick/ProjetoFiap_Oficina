using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.OrdensDeServico;

namespace Oficina.Api.Controllers;

// API do cliente: sem login, mas exige o CPF/CNPJ do dono da OS.
[ApiController]
[Route("api/acompanhamento")]
[AllowAnonymous]
public class AcompanhamentoController(OrdemServicoService service) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public Task<OrdemServicoResponse> Consultar(Guid id, [FromQuery] string documento, CancellationToken ct)
        => service.ConsultarPeloClienteAsync(id, documento, ct);

    [HttpPost("{id:guid}/aprovar")]
    public async Task<OrdemServicoResponse> Aprovar(Guid id, [FromQuery] string documento, CancellationToken ct)
    {
        await service.ConsultarPeloClienteAsync(id, documento, ct); // confere se a OS é dele
        return await service.AprovarOrcamentoAsync(id, ct);
    }

    [HttpPost("{id:guid}/recusar")]
    public async Task<OrdemServicoResponse> Recusar(Guid id, [FromQuery] string documento, CancellationToken ct)
    {
        await service.ConsultarPeloClienteAsync(id, documento, ct);
        return await service.RecusarOrcamentoAsync(id, ct);
    }
}
