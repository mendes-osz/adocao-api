using Microsoft.EntityFrameworkCore;

namespace AdocaoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Animal> Animais => Set<Animal>();
    public DbSet<Adotante> Adotantes => Set<Adotante>();
}

public class Animal
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Especie { get; set; } = string.Empty;
    public int Idade { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class Adotante
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}