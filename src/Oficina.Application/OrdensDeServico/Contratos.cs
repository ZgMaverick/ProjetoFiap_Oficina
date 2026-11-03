using Oficina.Domain.OrdensDeServico;

namespace Oficina.Application.OrdensDeServico;

public record CriarOrdemServicoRequest(
    string DocumentoCliente,
    string NomeCliente,
    string? TelefoneCliente,
    string? EmailCliente,
    string Placa,
    string Marca,
    string Modelo,
    int Ano,
    string RelatoCliente);

public record AdicionarPecaRequest(Guid PecaInsumoId, decimal Quantidade);

public record ItemResponse(string Tipo, string Descricao, decimal Quantidade, decimal PrecoUnitario, decimal Total);

public record HistoricoResponse(string Status, DateTime DataHora);

public record OrdemServicoResponse(
    Guid Id,
    Guid ClienteId,
    Guid VeiculoId,
    string Status,
    string RelatoCliente,
    string? Diagnostico,
    decimal ValorOrcamento,
    DateTime CriadaEm,
    IEnumerable<ItemResponse> Itens,
    IEnumerable<HistoricoResponse> Historico)
{
    public static OrdemServicoResponse De(OrdemDeServico os) => new(
        os.Id, os.ClienteId, os.VeiculoId, os.Status.ToString(), os.RelatoCliente,
        os.Diagnostico, os.ValorOrcamento, os.CriadaEm,
        os.Itens.Select(i => new ItemResponse(i.Tipo.ToString(), i.Descricao, i.Quantidade, i.PrecoUnitario, i.Total)),
        os.Historico.OrderBy(h => h.DataHora).Select(h => new HistoricoResponse(h.Status.ToString(), h.DataHora)));
}

public record TempoMedioResponse(int OrdensFinalizadas, double TempoMedioMinutos);
