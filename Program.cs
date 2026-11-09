using CatalogoGalacticoWebApi.Data;
using CatalogoGalacticoWebApi.Models;
using CatalogoGalacticoWebApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var conexion = builder.Configuration.GetConnectionString("DefaultConnection") ??
               throw new InvalidOperationException("Configure ConnectionStrings:DefaultConnection con dotnet user-secrets.");

// La versión explícita evita que las herramientas de EF necesiten abrir una conexión
// solo para descubrir la versión del servidor durante el diseño de migraciones.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(conexion, new MySqlServerVersion(new Version(8, 4, 9))));
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
