using System.Security.Cryptography;
using System.Text;

namespace QuitQ1_Hx.Utilities
{
    public static class SecurityUtilities
    {
        public static string GenerateSecureKey(int length = 32)
        {
            var key = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(key);
            return Convert.ToBase64String(key);
        }
    }
}
