using CatalogoGalacticoWebApi.Data;
using CatalogoGalacticoWebApi.Models;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var conexion = builder.Configuration.GetConnectionString("DefaultConnection") ??
               throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection con dotnet user-secrets.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(conexion, ServerVersion.AutoDetect(conexion)));
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<ISimulacionService, SimulacionService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();
builder.Services.AddSingleton<ICifradoService, CifradoService>();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/openapi/v1.json", "Catálogo Galáctico"));
}
app.MapControllers();
app.Run();
