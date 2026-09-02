using Microsoft.EntityFrameworkCore;
using ServicoFaturamento.Domain;

namespace ServicoFaturamento.Infrastructure;

public sealed class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options)
        : base(options)
    {
    }

    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNotaFiscal> ItensNotaFiscal => Set<ItemNotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.ToTable("NotasFiscais", "faturamento");
            entity.HasKey(nf => nf.Numero);
            entity.Property(nf => nf.Numero)
                  .ValueGeneratedOnAdd()
                  .UseIdentityColumn();
            entity.Property(nf => nf.Status).IsRequired();
            entity.HasMany(nf => nf.Itens)
                  .WithOne()
                  .HasForeignKey(item => item.NotaFiscalNumero)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemNotaFiscal>(entity =>
        {
            entity.ToTable("ItensNotaFiscal", "faturamento");
            entity.HasKey(item => new { item.NotaFiscalNumero, item.CodigoProduto });
            entity.Property(item => item.CodigoProduto).HasMaxLength(50).IsRequired();
            entity.Property(item => item.DescricaoProduto).HasMaxLength(200).IsRequired();
            entity.Property(item => item.Quantidade).HasPrecision(18, 3).IsRequired();
        });
    }
}
