using Microsoft.EntityFrameworkCore;

namespace AdocaoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Animal> Animais => Set<Animal>();
    public DbSet<Adotante> Adotantes => Set<Adotante>();
}

public record Animal(int Id, string Nome, string Especie, int Idade, string Status);
public record Adotante(int Id, string Nome, string Email);