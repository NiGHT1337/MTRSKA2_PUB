namespace Matroschka2.Crypto;

internal interface IRandomByteGenerator
{
    byte[] GetBytes(int size);
}
