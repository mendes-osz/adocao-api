using Microsoft.EntityFrameworkCore;
using AdocaoApi.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = "server=localhost;database=dbadocao;user=root;password=0987";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowReactApp");

// === Endpoints ===
app.MapGet("/Animais", async (AppDbContext db) => await db.Animais.ToListAsync());
app.MapPost("/Animais", async (Animal animal, AppDbContext db) =>
{
    db.Animais.Add(animal);
    await db.SaveChangesAsync();
    return Results.Created($"/Animais/{animal.Id}", animal);
});

app.MapGet("/Adotantes", async (AppDbContext db) => await db.Adotantes.ToListAsync());
app.MapPost("/Adotantes", async (Adotante adotante, AppDbContext db) =>
{
    db.Adotantes.Add(adotante);
    await db.SaveChangesAsync();
    return Results.Created($"/Adotantes/{adotante.Id}", adotante);
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();