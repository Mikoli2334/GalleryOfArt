using GalleryOfART.Persistence.Db;
using Microsoft.EntityFrameworkCore;
using GalleryOfART.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// читаем переменные окружения (их и так подхватит, но ок)
builder.Configuration.AddEnvironmentVariables();

// 1. Регистрируем контроллеры, Swagger и сервисы приложения
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IArtworkService, ArtworkService>();

// 2. Собираем connection string ОДИН РАЗ
var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433";
var db   = Environment.GetEnvironmentVariable("POSTGRES_DB") ;
var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ;
var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "YOUR_PASSWORD";

// если хочешь, можешь взять дефолт из appsettings, а env использовать как override,
// но чтобы не усложнять, оставим так:
var connectionString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";

// 3. Регистрируем DbContext (тоже до Build!)
builder.Services.AddDbContext<GalleryDbContext>(options =>
    options.UseNpgsql(connectionString));

// 4. Строим приложение
var app = builder.Build();

// 5. HTTP-pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();