namespace Matroschka2.Crypto;

internal interface IKeyDerivationService
{
    byte[] DeriveKey(string password, byte[] salt);
}
