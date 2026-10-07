using ProyectoCatalogoGalactico.Endpoints;
using ProyectoCatalogoGalactico.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<CatalogoService>();

// 1. Configuración del servicio CORS (Usa la política por defecto)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Catálogo Galáctico API v1"));
}

// 2. ACTIVAR CORS AQUÍ (Antes de los endpoints)
app.UseCors(); 

// 3. Registro de endpoints del API
var api = app.MapGroup(string.Empty);
api.MapPersonajeEndpoints();
api.MapCartaEndpoints();
api.MapEventoEndpoints();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

// 4. Iniciar la aplicación (Siempre al final)
app.Run();
