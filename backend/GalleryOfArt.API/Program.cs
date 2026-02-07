using GalleryOfART.Application.Services;
using GalleryOfART.Persistence.Db;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});


builder.Services.AddScoped<IArtworkService, ArtworkService>();
builder.Services.AddScoped<IArtistService, ArtistService>();

builder.Services.AddDbContext<GalleryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("FrontendDev");
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

app.Run();