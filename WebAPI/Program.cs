using Application.Interfaces;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Servicios del contenedor
builder.Services.AddControllers();

// Mas informacion sobre OpenAPI en https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configuracion del DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Inyeccion de dependencias
builder.Services.AddScoped<IEjemploService, EjemploService>();

var app = builder.Build();

// Pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
