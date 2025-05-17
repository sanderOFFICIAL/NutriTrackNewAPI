using Microsoft.EntityFrameworkCore;
using NutriTrack.Models;

var builder = WebApplication.CreateBuilder(args);

// Додаємо CORS і дозволяємо доступ з конкретних адрес фронтенду
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .WithOrigins(
                "http://192.168.0.183:5182",
                "http://localhost:59409",  // IP та порт твого Flutter Web додатка
                "http://localhost:5182"        // Локальний запуск (якщо є)
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

FirebaseService.Initialize();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Важливо! Спочатку вмикаємо CORS
app.UseCors();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();
