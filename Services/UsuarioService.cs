using CatalogoGalacticoWebApi.Data;
using CatalogoGalacticoWebApi.Dtos;
using CatalogoGalacticoWebApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CatalogoGalacticoWebApi.Services;
public class UsuarioService(AppDbContext db, IPasswordHasher<Usuario> hasher, ICifradoService cifrado) : IUsuarioService
{
    public async Task<Resultado<UsuarioResponse>> Crear(UsuarioInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Nombre) || string.IsNullOrWhiteSpace(input.Correo) || input.Contrasena.Length < 8)
            return Resultado<UsuarioResponse>.Fail("Nombre, correo y contraseña de 8 caracteres requeridos");
        var correo = input.Correo.Trim().ToLowerInvariant();
        var huella = cifrado.HuellaCorreo(correo);
        if (await db.Usuarios.AnyAsync(x => x.CorreoBusquedaHash == huella))
            return Resultado<UsuarioResponse>.Fail("Correo ya registrado", 409);
        var user = new Usuario { Nombre = input.Nombre.Trim(), CorreoBusquedaHash = huella, CorreoCifrado = cifrado.Cifrar(correo) };
        user.PasswordHash = hasher.HashPassword(user, input.Contrasena);
        db.Usuarios.Add(user);
        await db.SaveChangesAsync();
        return Resultado<UsuarioResponse>.Ok(new UsuarioResponse(user.Id, user.Nombre, correo), 201);
    }
    public async Task<Resultado<UsuarioResponse>> Obtener(int id)
    {
        var user = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return user == null
                   ? Resultado<UsuarioResponse>.Fail("Usuario no encontrado", 404)
                   : Resultado<UsuarioResponse>.Ok(new UsuarioResponse(user.Id, user.Nombre, cifrado.Descifrar(user.CorreoCifrado)));
    }
}
