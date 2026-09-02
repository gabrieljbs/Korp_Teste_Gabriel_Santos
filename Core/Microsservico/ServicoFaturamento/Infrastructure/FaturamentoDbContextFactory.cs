using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServicoFaturamento.Infrastructure;

public class FaturamentoDbContextFactory : IDesignTimeDbContextFactory<FaturamentoDbContext>
{
    public FaturamentoDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__FaturamentoConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Connection string 'ConnectionStrings__FaturamentoConnection' não encontrada. Defina em .env ou em variáveis de ambiente.");

        var optionsBuilder = new DbContextOptionsBuilder<FaturamentoDbContext>();
        optionsBuilder.UseNpgsql(connection);

        return new FaturamentoDbContext(optionsBuilder.Options);
    }
}
