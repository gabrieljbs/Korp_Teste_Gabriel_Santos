using Microsoft.EntityFrameworkCore;
using ServicoEstoque.Application;
using ServicoEstoque.Infrastructure;
using ServicoEstoque.Services;

// ── Carregar .env se presente ──────────────────────────────────────────────
var envFile = Path.Combine(AppContext.BaseDirectory, ".env");
if (!File.Exists(envFile))
    envFile = Path.Combine(Directory.GetCurrentDirectory(), ".env");

if (File.Exists(envFile))
{
    foreach (var line in File.ReadAllLines(envFile))
    {
        var trimmed = line.Trim();
        if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith('#')) continue;
        var idx = trimmed.IndexOf('=');
        if (idx <= 0) continue;
        var key = trimmed[..idx].Trim();
        var val = trimmed[(idx + 1)..].Trim().Trim('"');
        if (Environment.GetEnvironmentVariable(key) is null)
            Environment.SetEnvironmentVariable(key, val);
    }
}

var builder = WebApplication.CreateBuilder(args);

// ── CORS ───────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins(
                "http://localhost:4200",
                "http://127.0.0.1:4200",
                "http://localhost:5002",   // faturamento pode chamar diretamente
                "http://127.0.0.1:5002")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// ── Controllers + Swagger ──────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ── Banco de Dados — Estoque ───────────────────────────────────────────────
builder.Services.AddDbContext<EstoqueDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException(
            "Connection string 'DefaultConnection' não configurada.")));

// ── Repositório e Serviço ──────────────────────────────────────────────────
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<EstoqueService>();

var app = builder.Build();

// ── Migrations automáticas na inicialização ────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<EstoqueDbContext>();
    db.Database.Migrate();
}

// ── Pipeline ───────────────────────────────────────────────────────────────
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Serviço de Estoque API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
