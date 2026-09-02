using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ServicoEstoque.Infrastructure;

public class EstoqueDbContextFactory : IDesignTimeDbContextFactory<EstoqueDbContext>
{
    public EstoqueDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (string.IsNullOrWhiteSpace(connection))
            throw new InvalidOperationException("Connection string 'ConnectionStrings__DefaultConnection' não encontrada. Defina em .env ou em variáveis de ambiente.");

        var optionsBuilder = new DbContextOptionsBuilder<EstoqueDbContext>();
        optionsBuilder.UseNpgsql(connection);

        return new EstoqueDbContext(optionsBuilder.Options);
    }
}