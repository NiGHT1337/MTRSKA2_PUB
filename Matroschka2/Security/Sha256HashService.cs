using System.Security.Cryptography;
using System.Text;

namespace Matroschka2.Security;

internal sealed class Sha256HashService : IHashService
{
    public string Hash(string input)
    {
        byte[] bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(bytes);
    }
}
