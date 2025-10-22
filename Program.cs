using Microsoft.EntityFrameworkCore;
using AdocaoApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Conexão com o MySQL
var connectionString = "server=localhost;database=dbadocao;user=root;password=0987";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Habilitar CORS (para o React poder acessar)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddControllers();
var app = builder.Build();

app.UseCors("AllowReactApp");
app.MapControllers();

// Criar banco automaticamente, se não existir
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.Run();
