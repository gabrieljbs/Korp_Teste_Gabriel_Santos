using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Domain;

namespace ServicoEstoque.Infrastructure;

public sealed class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos", "estoque");
            entity.HasKey(produto => produto.Codigo);
            entity.Property(produto => produto.Codigo).HasMaxLength(50).IsRequired();
            entity.Property(produto => produto.Descricao).HasMaxLength(200).IsRequired();
            entity.Property(produto => produto.Saldo).HasPrecision(18, 3).IsRequired();
        });
    }
}