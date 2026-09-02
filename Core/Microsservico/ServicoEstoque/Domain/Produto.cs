namespace ServicoEstoque.Domain;

public sealed class Produto
{
    public string Codigo { get; private set; } = string.Empty;
    public string Descricao { get; private set; } = string.Empty;
    public decimal Saldo { get; private set; }

    private Produto()
    {
    }

    public Produto(string codigo, string descricao, decimal saldo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("O código é obrigatório.", nameof(codigo));

        if (string.IsNullOrWhiteSpace(descricao))
            throw new ArgumentException("A descrição é obrigatória.", nameof(descricao));

        if (saldo < 0)
            throw new ArgumentOutOfRangeException(nameof(saldo), "O saldo não pode ser negativo.");

        Codigo = codigo.Trim();
        Descricao = descricao.Trim();
        Saldo = saldo;
    }

    public void AlterarSaldo(decimal quantidade)
    {
        if (quantidade == 0)
            throw new ArgumentException("A quantidade deve ser diferente de zero.", nameof(quantidade));

        if (Saldo + quantidade < 0)
            throw new InvalidOperationException("Saldo insuficiente para a operação.");

        Saldo += quantidade;
    }
}