using System.Buffers.Binary;
using System.Diagnostics;
using System.Security.Cryptography;
using Matroschka2.UI;

namespace Matroschka2.Crypto;

internal sealed class FileEncryptionService
{
    private readonly IKeyDerivationService _keyDerivationService;
    private readonly IRandomByteGenerator _randomByteGenerator;

    public FileEncryptionService(
        IKeyDerivationService keyDerivationService,
        IRandomByteGenerator randomByteGenerator)
    {
        _keyDerivationService = keyDerivationService;
        _randomByteGenerator = randomByteGenerator;
    }

    public void EncryptFile(
        string inputFile,
        string outputFile,
        string password,
        IProgressReporter progressReporter)
    {
        Console.WriteLine("    ______                            __  _              \r\n   / ____/___  ____________  ______  / /_(_)___  ____ __ \r\n  / __/ / __ \\/ ___/ ___/ / / / __ \\/ __/ / __ \\/ __ `(_)\r\n / /___/ / / / /__/ /  / /_/ / /_/ / /_/ / / / / /_/ /   \r\n/_____/_/ /_/\\___/_/   \\__, / .___/\\__/_/_/ /_/\\__, (_)  \r\n                      /____/_/                /____/     ");

        byte[] salt = _randomByteGenerator.GetBytes(CryptoConstants.SaltSize);
        byte[] key = _keyDerivationService.DeriveKey(password, salt);
        byte[] noncePrefix = _randomByteGenerator.GetBytes(CryptoConstants.NonceSize);

        long totalBytes = new FileInfo(inputFile).Length;
        long bytesProcessed = 0;
        long chunkIndex = 0;
        var stopwatch = Stopwatch.StartNew();

        using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
        using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

        WriteHeader(outputStream, salt, noncePrefix);

        using var aes = new AesGcm(key, CryptoConstants.TagSize);
        byte[] buffer = new byte[CryptoConstants.BufferSize];
        byte[] encryptedBuffer = new byte[CryptoConstants.BufferSize];
        byte[] tag = new byte[CryptoConstants.TagSize];
        byte[] lengthBuffer = new byte[sizeof(int)];

        int bytesRead;
        while ((bytesRead = inputStream.Read(buffer, 0, buffer.Length)) > 0)
        {
            byte[] nonce = BuildNonce(noncePrefix, chunkIndex++);
            aes.Encrypt(nonce, buffer.AsSpan(0, bytesRead), encryptedBuffer.AsSpan(0, bytesRead), tag);

            BinaryPrimitives.WriteInt32LittleEndian(lengthBuffer, bytesRead);
            outputStream.Write(lengthBuffer);
            outputStream.Write(tag);
            outputStream.Write(encryptedBuffer, 0, bytesRead);

            bytesProcessed += bytesRead;
            progressReporter.WriteProgressWithEta(bytesProcessed, totalBytes, stopwatch.Elapsed);
        }

        Console.WriteLine("\nFile encrypted successfully.");
    }

    public void DecryptFile(
        string inputFile,
        string outputFile,
        string password,
        IProgressReporter progressReporter)
    {
        Console.WriteLine("    ____                             __  _              \r\n   / __ \\___  ____________  ______  / /_(_)___  ____ __ \r\n  / / / / _ \\/ ___/ ___/ / / / __ \\/ __/ / __ \\/ __ `(_)\r\n / /_/ /  __/ /__/ /  / /_/ / /_/ / /_/ / / / / /_/ /   \r\n/_____/\\___/\\___/_/   \\__, / .___/\\__/_/_/ /_/\\__, (_)  \r\n                     /____/_/                /____/     ");

        using var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read);
        using var outputStream = new FileStream(outputFile, FileMode.Create, FileAccess.Write);

        if (HasCurrentFormatHeader(inputStream))
        {
            DecryptCurrentFormat(inputStream, outputStream, password, progressReporter);
        }
        else
        {
            inputStream.Position = 0;
            DecryptLegacyFormat(inputStream, outputStream, password, progressReporter);
        }

        Console.WriteLine("\nFile decrypted successfully.");
    }

    private void DecryptCurrentFormat(
        FileStream inputStream,
        FileStream outputStream,
        string password,
        IProgressReporter progressReporter)
    {
        byte[] salt = ReadExactly(inputStream, CryptoConstants.SaltSize);
        byte[] noncePrefix = ReadExactly(inputStream, CryptoConstants.NonceSize);
        byte[] key = _keyDerivationService.DeriveKey(password, salt);

        long totalBytes = inputStream.Length - inputStream.Position;
        long bytesProcessed = 0;
        long chunkIndex = 0;
        var stopwatch = Stopwatch.StartNew();

        using var aes = new AesGcm(key, CryptoConstants.TagSize);
        byte[] lengthBuffer = new byte[sizeof(int)];
        byte[] encryptedBuffer = new byte[CryptoConstants.BufferSize];
        byte[] decryptedBuffer = new byte[CryptoConstants.BufferSize];

        while (inputStream.Position < inputStream.Length)
        {
            ReadExactly(inputStream, lengthBuffer);
            int chunkLength = BinaryPrimitives.ReadInt32LittleEndian(lengthBuffer);
            if (chunkLength <= 0 || chunkLength > CryptoConstants.BufferSize)
            {
                throw new InvalidDataException("Encrypted file contains an invalid chunk length.");
            }

            byte[] tag = ReadExactly(inputStream, CryptoConstants.TagSize);
            ReadExactly(inputStream, encryptedBuffer.AsSpan(0, chunkLength));

            byte[] nonce = BuildNonce(noncePrefix, chunkIndex++);
            aes.Decrypt(nonce, encryptedBuffer.AsSpan(0, chunkLength), tag, decryptedBuffer.AsSpan(0, chunkLength));
            outputStream.Write(decryptedBuffer, 0, chunkLength);

            bytesProcessed = inputStream.Position - CryptoConstants.Magic.Length - 1 - CryptoConstants.SaltSize - CryptoConstants.NonceSize;
            progressReporter.WriteProgressWithEta(bytesProcessed, totalBytes, stopwatch.Elapsed);
        }
    }

    private void DecryptLegacyFormat(
        FileStream inputStream,
        FileStream outputStream,
        string password,
        IProgressReporter progressReporter)
    {
        byte[] salt = ReadExactly(inputStream, CryptoConstants.SaltSize);
        byte[] nonce = ReadExactly(inputStream, CryptoConstants.NonceSize);
        byte[] key = _keyDerivationService.DeriveKey(password, salt);

        long totalBytes = inputStream.Length - CryptoConstants.SaltSize - CryptoConstants.NonceSize;
        long bytesProcessed = 0;
        var stopwatch = Stopwatch.StartNew();

        using var aes = new AesGcm(key, CryptoConstants.TagSize);
        byte[] encryptedBuffer = new byte[CryptoConstants.BufferSize];
        byte[] decryptedBuffer = new byte[CryptoConstants.BufferSize];

        while (inputStream.Position < inputStream.Length)
        {
            byte[] tag = ReadExactly(inputStream, CryptoConstants.TagSize);
            int bytesRead = inputStream.Read(encryptedBuffer, 0, encryptedBuffer.Length);
            if (bytesRead == 0)
            {
                throw new InvalidDataException("File corruption detected.");
            }

            aes.Decrypt(nonce, encryptedBuffer.AsSpan(0, bytesRead), tag, decryptedBuffer.AsSpan(0, bytesRead));
            outputStream.Write(decryptedBuffer, 0, bytesRead);

            bytesProcessed += bytesRead + CryptoConstants.TagSize;
            progressReporter.WriteProgressWithEta(bytesProcessed, totalBytes, stopwatch.Elapsed);
        }
    }

    private static void WriteHeader(Stream outputStream, byte[] salt, byte[] noncePrefix)
    {
        outputStream.Write(CryptoConstants.Magic);
        outputStream.WriteByte(CryptoConstants.FormatVersion);
        outputStream.Write(salt);
        outputStream.Write(noncePrefix);
    }

    private static bool HasCurrentFormatHeader(Stream inputStream)
    {
        if (inputStream.Length < CryptoConstants.Magic.Length + 1)
        {
            return false;
        }

        byte[] magic = ReadExactly(inputStream, CryptoConstants.Magic.Length);
        int version = inputStream.ReadByte();

        return magic.SequenceEqual(CryptoConstants.Magic) && version == CryptoConstants.FormatVersion;
    }

    private static byte[] BuildNonce(byte[] noncePrefix, long chunkIndex)
    {
        byte[] nonce = new byte[CryptoConstants.NonceSize];
        Array.Copy(noncePrefix, nonce, nonce.Length);
        BinaryPrimitives.WriteInt64LittleEndian(nonce.AsSpan(4), chunkIndex);
        return nonce;
    }

    private static byte[] ReadExactly(Stream stream, int length)
    {
        byte[] buffer = new byte[length];
        ReadExactly(stream, buffer);
        return buffer;
    }

    private static void ReadExactly(Stream stream, Span<byte> buffer)
    {
        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int bytesRead = stream.Read(buffer[totalRead..]);
            if (bytesRead == 0)
            {
                throw new InvalidDataException("Unexpected end of encrypted file.");
            }

            totalRead += bytesRead;
        }
    }
}
