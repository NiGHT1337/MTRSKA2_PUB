using System.Security.Cryptography;

namespace Matroschka2.Crypto;

internal sealed class SecureRandomByteGenerator : IRandomByteGenerator
{
    public byte[] GetBytes(int size)
    {
        byte[] bytes = new byte[size];
        RandomNumberGenerator.Fill(bytes);
        return bytes;
    }
}
