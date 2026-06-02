namespace Matroschka2.Crypto;

internal static class CryptoConstants
{
    public const int KeySize = 32;
    public const int SaltSize = 16;
    public const int NonceSize = 12;
    public const int TagSize = 16;
    public const int BufferSize = 4096 * 16;

    public static readonly byte[] Magic = "MTRSKA2"u8.ToArray();
    public const byte FormatVersion = 1;
}
