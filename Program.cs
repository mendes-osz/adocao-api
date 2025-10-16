using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MySqlConnector;

namespace AdocaoApi
{
    public record Animal(int Id, string Nome, string Especie, int Idade, string Status);
    public record Adotante(int Id, string Nome, string Email);

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Animal> Animais => Set<Animal>();
        public DbSet<Adotante> Adotantes => Set<Adotante>();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = "server=localhost;database=dbadocao;user=root;password=0987";
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            );

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseSwagger();
            app.UseSwaggerUI();

            app.MapGet("/animais", async (AppDbContext db) =>
            {
                try
                {
                    return Results.Ok(await db.Animais.ToListAsync());
                }
                catch (MySqlException)
                {
                    return Results.Problem("Erro de conexão com o banco de dados.");
                }
                catch (InvalidOperationException)
                {
                    return Results.Problem("Erro interno ao acessar os dados.");
                }
            });

            app.MapGet("/animais/{id}", async (int id, AppDbContext db) =>
            {
                try
                {
                    var animal = await db.Animais.FindAsync(id);
                    return animal is not null ? Results.Ok(animal) : Results.NotFound("Animal não encontrado.");
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao consultar o banco de dados.");
                }
            });

            app.MapPost("/animais", async (Animal animal, AppDbContext db) =>
            {
                try
                {
                    db.Animais.Add(animal);
                    await db.SaveChangesAsync();
                    return Results.Created($"/animais/{animal.Id}", animal);
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao salvar o animal no banco de dados. Verifique os dados informados.");
                }
                catch (MySqlException)
                {
                    return Results.Problem("Falha na conexão com o banco de dados.");
                }
            });

            app.MapPut("/animais/{id}", async (int id, Animal dadosAtualizados, AppDbContext db) =>
            {
                try
                {
                    var animal = await db.Animais.FindAsync(id);
                    if (animal is null) return Results.NotFound("Animal não encontrado.");

                    var atualizado = animal with
                    {
                        Nome = dadosAtualizados.Nome,
                        Especie = dadosAtualizados.Especie,
                        Idade = dadosAtualizados.Idade,
                        Status = dadosAtualizados.Status
                    };

                    db.Entry(animal).CurrentValues.SetValues(atualizado);
                    await db.SaveChangesAsync();
                    return Results.Ok(atualizado);
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao atualizar os dados no banco.");
                }
            });

            app.MapDelete("/animais/{id}", async (int id, AppDbContext db) =>
            {
                try
                {
                    var animal = await db.Animais.FindAsync(id);
                    if (animal is null) return Results.NotFound("Animal não encontrado.");

                    db.Animais.Remove(animal);
                    await db.SaveChangesAsync();
                    return Results.Ok(animal);
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao remover o registro do banco de dados.");
                }
            });

            app.MapGet("/adotantes", async (AppDbContext db) =>
            {
                try
                {
                    return Results.Ok(await db.Adotantes.ToListAsync());
                }
                catch (MySqlException)
                {
                    return Results.Problem("Erro de conexão com o banco de dados.");
                }
            });

            app.MapGet("/adotantes/{id}", async (int id, AppDbContext db) =>
            {
                try
                {
                    var adotante = await db.Adotantes.FindAsync(id);
                    return adotante is not null ? Results.Ok(adotante) : Results.NotFound("Adotante não encontrado.");
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao consultar o banco de dados.");
                }
            });

            app.MapPost("/adotantes", async (Adotante adotante, AppDbContext db) =>
            {
                try
                {
                    db.Adotantes.Add(adotante);
                    await db.SaveChangesAsync();
                    return Results.Created($"/adotantes/{adotante.Id}", adotante);
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao salvar o adotante no banco de dados.");
                }
            });

            app.MapPut("/adotantes/{id}", async (int id, Adotante dadosAtualizados, AppDbContext db) =>
            {
                try
                {
                    var adotante = await db.Adotantes.FindAsync(id);
                    if (adotante is null) return Results.NotFound("Adotante não encontrado.");

                    var atualizado = adotante with
                    {
                        Nome = dadosAtualizados.Nome,
                        Email = dadosAtualizados.Email
                    };

                    db.Entry(adotante).CurrentValues.SetValues(atualizado);
                    await db.SaveChangesAsync();
                    return Results.Ok(atualizado);
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao atualizar o registro no banco de dados.");
                }
            });

            app.MapDelete("/adotantes/{id}", async (int id, AppDbContext db) =>
            {
                try
                {
                    var adotante = await db.Adotantes.FindAsync(id);
                    if (adotante is null) return Results.NotFound("Adotante não encontrado.");

                    db.Adotantes.Remove(adotante);
                    await db.SaveChangesAsync();
                    return Results.Ok(adotante);
                }
                catch (DbUpdateException)
                {
                    return Results.Problem("Erro ao excluir o adotante do banco de dados.");
                }
            });

            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }
                catch (MySqlException)
                {
                    Console.WriteLine("Erro ao conectar ao banco. Verifique as credenciais.");
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("Erro interno ao criar o banco de dados.");
                }
            }

            app.Run();
        }
    }
}