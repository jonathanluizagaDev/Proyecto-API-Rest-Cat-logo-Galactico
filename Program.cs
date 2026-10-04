using ProyectoCatalogoGalactico.Endpoints;
using ProyectoCatalogoGalactico.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<CatalogoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/openapi/v1.json", "Catálogo Galáctico API v1"));
}

var api = app.MapGroup("/api");
api.MapPersonajeEndpoints();
api.MapCartaEndpoints();
api.MapEventoEndpoints();

app.MapGet("/", () => Results.Redirect("/swagger"))
    .ExcludeFromDescription();

app.Run();
