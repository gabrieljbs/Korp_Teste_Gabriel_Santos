using ServicoFaturamento.Domain;

namespace ServicoFaturamento.DTOs;

public sealed record NotaFiscalDto
{
    public long Numero { get; init; }
    public StatusNotaFiscal Status { get; init; }
    public IReadOnlyCollection<ItemNotaFiscalDto> Itens { get; init; } = [];
}