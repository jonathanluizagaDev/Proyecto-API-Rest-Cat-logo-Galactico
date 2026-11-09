using System.Security.Cryptography;
using System.Text;
namespace CatalogoGalacticoWebApi.Services;
public class CifradoService : ICifradoService
{
    private readonly byte[] clave;
    public CifradoService(IConfiguration configuracion)
    {
        var secreto = configuracion["Cifrado:Clave"] ?? throw new InvalidOperationException("Falta Cifrado:Clave en user-secrets");
        clave = Convert.FromBase64String(secreto);
        if (clave.Length != 32)
            throw new InvalidOperationException("Cifrado:Clave debe contener 32 bytes en Base64");
    }
    public string Cifrar(string texto)
    {
        byte[] nonce = RandomNumberGenerator.GetBytes(12), datos = Encoding.UTF8.GetBytes(texto), cifrado = new byte[datos.Length],
               tag = new byte[16];
        using (var aes = new AesGcm(clave, 16)) aes.Encrypt(nonce, datos, cifrado, tag);
        return Convert.ToBase64String(nonce.Concat(tag).Concat(cifrado).ToArray());
    }
    public string Descifrar(string texto)
    {
        var datos = Convert.FromBase64String(texto);
        if (datos.Length < 28)
            throw new CryptographicException("Campo cifrado inválido");
        var plano = new byte[datos.Length - 28];
        using (var aes = new AesGcm(clave, 16)) aes.Decrypt(datos[..12], datos[28..], datos[12..28], plano);
        return Encoding.UTF8.GetString(plano);
    }
    // HMAC evita guardar correo en claro y permite buscar duplicados de forma determinista.
    public string HuellaCorreo(string correo) => Convert.ToHexString(
        HMACSHA256.HashData(clave, Encoding.UTF8.GetBytes(correo.Trim().ToLowerInvariant())));
}
