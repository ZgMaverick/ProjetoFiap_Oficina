using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oficina.Application.OrdensDeServico;
using Oficina.Domain.OrdensDeServico;

namespace Oficina.Api.Controllers;

public record DiagnosticoRequest(string Diagnostico);
public record AdicionarServicoRequest(Guid ServicoId);

[ApiController]
[Route("api/ordens-servico")]
[Authorize]
public class OrdensServicoController(OrdemServicoService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<OrdemServicoResponse>> Criar(CriarOrdemServicoRequest req, CancellationToken ct)
    {
        var os = await service.CriarAsync(req, ct);
        return CreatedAtAction(nameof(Obter), new { id = os.Id }, os);
    }

    [HttpGet]
    public Task<List<OrdemServicoResponse>> Listar([FromQuery] StatusOrdemServico? status, CancellationToken ct)
        => service.ListarAsync(status, ct);

    [HttpGet("{id:guid}")]
    public Task<OrdemServicoResponse> Obter(Guid id, CancellationToken ct) => service.ObterAsync(id, ct);

    [HttpPost("{id:guid}/iniciar-diagnostico")]
    public Task<OrdemServicoResponse> IniciarDiagnostico(Guid id, CancellationToken ct)
        => service.IniciarDiagnosticoAsync(id, ct);

    [HttpPut("{id:guid}/diagnostico")]
    public Task<OrdemServicoResponse> RegistrarDiagnostico(Guid id, DiagnosticoRequest req, CancellationToken ct)
        => service.RegistrarDiagnosticoAsync(id, req.Diagnostico, ct);

    [HttpPost("{id:guid}/servicos")]
    public Task<OrdemServicoResponse> AdicionarServico(Guid id, AdicionarServicoRequest req, CancellationToken ct)
        => service.AdicionarServicoAsync(id, req.ServicoId, ct);

    [HttpPost("{id:guid}/pecas-insumos")]
    public Task<OrdemServicoResponse> AdicionarPecaInsumo(Guid id, AdicionarPecaRequest req, CancellationToken ct)
        => service.AdicionarPecaInsumoAsync(id, req, ct);

    [HttpPost("{id:guid}/enviar-orcamento")]
    public Task<OrdemServicoResponse> EnviarOrcamento(Guid id, CancellationToken ct)
        => service.EnviarOrcamentoAsync(id, ct);

    [HttpPost("{id:guid}/cancelar")]
    public Task<OrdemServicoResponse> Cancelar(Guid id, CancellationToken ct) => service.CancelarAsync(id, ct);

    [HttpPost("{id:guid}/finalizar")]
    public Task<OrdemServicoResponse> Finalizar(Guid id, CancellationToken ct) => service.FinalizarAsync(id, ct);

    [HttpPost("{id:guid}/entregar")]
    public Task<OrdemServicoResponse> Entregar(Guid id, CancellationToken ct) => service.EntregarAsync(id, ct);

    [HttpGet("tempo-medio-execucao")]
    public Task<TempoMedioResponse> TempoMedio(CancellationToken ct) => service.TempoMedioDeExecucaoAsync(ct);
}
