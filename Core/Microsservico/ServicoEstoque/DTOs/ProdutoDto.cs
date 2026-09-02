namespace ServicoEstoque.DTOs;

public sealed record ProdutoDto
{
    public string Codigo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public decimal Saldo { get; init; }
}