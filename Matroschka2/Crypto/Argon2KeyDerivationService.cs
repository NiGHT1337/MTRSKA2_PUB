using System.Text;
using Konscious.Security.Cryptography;

namespace Matroschka2.Crypto;

internal sealed class Argon2KeyDerivationService : IKeyDerivationService
{
    public byte[] DeriveKey(string password, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            MemorySize = 65536,
            Iterations = 4
        };

        return argon2.GetBytes(CryptoConstants.KeySize);
    }
}
