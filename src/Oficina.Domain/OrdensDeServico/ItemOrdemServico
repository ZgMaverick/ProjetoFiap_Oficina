namespace Oficina.Domain.OrdensDeServico;

public enum TipoItem
{
    Servico = 1,
    PecaInsumo = 2
}

// Uma linha do orçamento: um serviço ou uma peça/insumo.
public class ItemOrdemServico
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public TipoItem Tipo { get; private set; }
    public Guid ReferenciaId { get; private set; } // Id do Servico ou da PecaInsumo
    public string Descricao { get; private set; } = "";
    public decimal Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }
    public decimal Total => Quantidade * PrecoUnitario;

    private ItemOrdemServico() { }

    internal ItemOrdemServico(TipoItem tipo, Guid referenciaId, string descricao,
        decimal quantidade, decimal precoUnitario)
    {
        Tipo = tipo;
        ReferenciaId = referenciaId;
        Descricao = descricao;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }
}
