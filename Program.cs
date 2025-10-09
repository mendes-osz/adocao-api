using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

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

            app.MapGet("/animais", async (AppDbContext db) => await db.Animais.ToListAsync());

            app.MapGet("/animais/{id}", async (int id, AppDbContext db) =>
            {
                var animal = await db.Animais.FindAsync(id);
                return animal is not null ? Results.Ok(animal) : Results.NotFound();
            });

            app.MapPost("/animais", async (Animal animal, AppDbContext db) =>
            {
                db.Animais.Add(animal);
                await db.SaveChangesAsync();
                return Results.Created($"/animais/{animal.Id}", animal);
            });

            app.MapDelete("/animais/{id}", async (int id, AppDbContext db) =>
            {
                var animal = await db.Animais.FindAsync(id);
                if (animal is null) return Results.NotFound();
                db.Animais.Remove(animal);
                await db.SaveChangesAsync();
                return Results.Ok(animal);
            });

            app.MapGet("/adotantes", async (AppDbContext db) => await db.Adotantes.ToListAsync());

            app.MapGet("/adotantes/{id}", async (int id, AppDbContext db) =>
            {
                var adotante = await db.Adotantes.FindAsync(id);
                return adotante is not null ? Results.Ok(adotante) : Results.NotFound();
            });

            app.MapPost("/adotantes", async (Adotante adotante, AppDbContext db) =>
            {
                db.Adotantes.Add(adotante);
                await db.SaveChangesAsync();
                return Results.Created($"/adotantes/{adotante.Id}", adotante);
            });

            app.MapDelete("/adotantes/{id}", async (int id, AppDbContext db) =>
            {
                var adotante = await db.Adotantes.FindAsync(id);
                if (adotante is null) return Results.NotFound();
                db.Adotantes.Remove(adotante);
                await db.SaveChangesAsync();
                return Results.Ok(adotante);
            });

            app.Run();
        }
    }
}