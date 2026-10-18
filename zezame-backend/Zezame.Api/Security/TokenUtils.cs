using System.Security.Cryptography;
using System.Text;

namespace Zezame.Api.Security {
    /// <summary>
    /// Utilidades para tokens opacos: generación aleatoria apta para URL y hash
    /// SHA-256 (solo se persiste el hash, nunca el token en claro).
    /// </summary>
    public static class TokenUtils {
        public static string GenerateUrlToken()
            => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        public static string Hash(string raw)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw)));
    }
}
