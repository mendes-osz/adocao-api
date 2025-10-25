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

app.MapGet("/Animais", async (AppDbContext db) =>
{
    try
    {
        var animais = await db.Animais.ToListAsync();
        return Results.Ok(animais);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem($"Erro ao acessar o banco de dados: {dbEx.Message}");
    }
    catch (InvalidOperationException opEx)
    {
        return Results.Problem($"Operação inválida: {opEx.Message}");
    }
});

app.MapGet("/Animais/{id}", async (int id, AppDbContext db) =>
{
    try
    {
        var animal = await db.Animais.FindAsync(id);
        return animal is not null
            ? Results.Ok(animal)
            : Results.NotFound(new { mensagem = "Animal não encontrado." });
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem($"Erro ao consultar o banco: {dbEx.Message}");
    }
});

app.MapPost("/Animais", async (Animal animal, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(animal.Nome) || string.IsNullOrWhiteSpace(animal.Especie))
        return Results.BadRequest(new { mensagem = "Nome e espécie são obrigatórios." });

    try
    {
        db.Animais.Add(animal);
        await db.SaveChangesAsync();
        return Results.Created($"/Animais/{animal.Id}", animal);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem($"Erro ao salvar no banco de dados: {dbEx.Message}");
    }
});

app.MapPut("/Animais/{id}", async (int id, Animal updatedAnimal, AppDbContext db) =>
{
    try
    {
        var animalExistente = await db.Animais.FindAsync(id);
        if (animalExistente is null)
            return Results.NotFound($"Animal com ID {id} não encontrado.");

        animalExistente.Nome = updatedAnimal.Nome;
        animalExistente.Especie = updatedAnimal.Especie;
        animalExistente.Idade = updatedAnimal.Idade;
        animalExistente.Status = updatedAnimal.Status;

        await db.SaveChangesAsync();
        return Results.Ok(animalExistente);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Results.Conflict("Erro de concorrência ao atualizar o registro. Tente novamente.");
    }
    catch (DbUpdateException ex)
    {
        return Results.BadRequest($"Erro ao atualizar o animal: {ex.InnerException?.Message ?? ex.Message}");
    }
    catch (ArgumentNullException)
    {
        return Results.BadRequest("Os dados do animal não podem ser nulos.");
    }
});


app.MapDelete("/Animais/{id}", async (int id, AppDbContext db) =>
{
    try
    {
        var animal = await db.Animais.FindAsync(id);
        if (animal is null)
            return Results.NotFound(new { mensagem = "Animal não encontrado para exclusão." });

        db.Animais.Remove(animal);
        await db.SaveChangesAsync();

        return Results.Ok(new { mensagem = "Animal removido com sucesso." });
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem($"Erro ao excluir do banco de dados: {dbEx.Message}");
    }
});

app.MapGet("/Adotantes", async (AppDbContext db) =>
{
    try
    {
        var adotantes = await db.Adotantes.ToListAsync();
        return Results.Ok(adotantes);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem($"Erro ao acessar o banco: {dbEx.Message}");
    }
});

app.MapPost("/Adotantes", async (Adotante adotante, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(adotante.Nome) || string.IsNullOrWhiteSpace(adotante.Email))
        return Results.BadRequest(new { mensagem = "Nome e email são obrigatórios." });

    try
    {
        db.Adotantes.Add(adotante);
        await db.SaveChangesAsync();
        return Results.Created($"/Adotantes/{adotante.Id}", adotante);
    }
    catch (DbUpdateException dbEx)
    {
        return Results.Problem($"Erro ao salvar adotante no banco: {dbEx.Message}");
    }
});

try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
    }
}
catch (InvalidOperationException opEx)
{
    Console.WriteLine($"Erro ao inicializar o banco: {opEx.Message}");
}

app.Run();