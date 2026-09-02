namespace ServicoFaturamento.DTOs;

public sealed record ItemNotaFiscalDto
{
    public string CodigoProduto { get; init; } = string.Empty;
    public string DescricaoProduto { get; init; } = string.Empty;
    public decimal Quantidade { get; init; }
}