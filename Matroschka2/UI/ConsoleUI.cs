using System.Text;
using Matroschka2.Models;

namespace Matroschka2.UI;

internal sealed class ConsoleUI
{
    private const string EncryptedFileSuffix = "_mtrks2";

    public void ClearWithHeader()
    {
        Console.Clear();
        ShowHeader();
    }

    public void ShowHeader()
    {
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("   _____          __                             .__     __               _________               ________  \r\n  /     \\ _____ _/  |________  ____  ______ ____ |  |__ |  | _______     /   _____/ ____   ____   \\_____  \\ \r\n /  \\ /  \\\\__  \\\\   __\\_  __ \\/  _ \\/  ___// ___\\|  |  \\|  |/ /\\__  \\    \\_____  \\_/ __ \\_/ ___\\   /  ____/ \r\n/    Y    \\/ __ \\|  |  |  | \\(  <_> )___ \\\\  \\___|   Y  \\    <  / __ \\_  /        \\  ___/\\  \\___  /       \\ \r\n\\____|__  (____  /__|  |__|   \\____/____  >\\___  >___|  /__|_ \\(____  / /_______  /\\___  >\\___  > \\_______ \\\r\n        \\/     \\/                       \\/     \\/     \\/     \\/     \\/          \\/     \\/     \\/          \\/");
        Console.WriteLine("________________________________________________________________________________________________________________\n\n");
        Console.ResetColor();
    }

    public OperationMode? ReadMode()
    {
        Console.Write("Enter mode 1). encrypt | 2). decrypt): ");
        string? mode = Console.ReadLine()?.Trim().ToLowerInvariant();

        return mode switch
        {
            "1" or "encrypt" => OperationMode.Encrypt,
            "2" or "decrypt" => OperationMode.Decrypt,
            _ => null
        };
    }

    public CryptoOperationRequest ReadOperationRequest(OperationMode mode)
    {
        Console.Write("Enter input file path: ");
        string inputFile = ReadRequiredLine();
        Console.WriteLine();

        string outputFile = CreateOutputFilePath(inputFile, mode);
        if (ReadYesNo("Do you want to set a custom output path? (y/n): "))
        {
            bool appendEncryptedSuffix = mode == OperationMode.Encrypt
                && ReadYesNo($"Do you want to append the default encrypted file suffix ({EncryptedFileSuffix})? (y/n): ");
            Console.WriteLine();

            if (mode == OperationMode.Encrypt && !appendEncryptedSuffix)
            {
                WriteWarning("Warning: Set an alternative file name. If you use the original input path, the file can be overwritten.");
                WriteWarning("Recommendation: Give the file an extension or suffix that marks it as encrypted so it is easier to recognize.");
            }

            Console.Write("Enter output file path: ");
            outputFile = ReadRequiredLine();

            if (appendEncryptedSuffix && !outputFile.EndsWith(EncryptedFileSuffix, StringComparison.OrdinalIgnoreCase))
            {
                outputFile += EncryptedFileSuffix;
            }
        }

        Console.WriteLine($"Output file path: {outputFile}");
        Console.WriteLine();

        Console.Write("Enter password: ");
        string password = ReadPassword();

        return new CryptoOperationRequest(mode, inputFile, outputFile, password);
    }

    public void ShowError(string message)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("___________                           \r\n\\_   _____/_____________  ___________ \r\n |    __)_\\_  __ \\_  __ \\/  _ \\_  __ \\\r\n |        \\|  | \\/|  | \\(  <_> )  | \\/\r\n/_______  /|__|   |__|   \\____/|__|   \r\n        \\/                            ");
        Console.ResetColor();
        Console.WriteLine(message);
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    private static void WriteWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void WriteMode(OperationMode mode)
    {
        Console.WriteLine(mode == OperationMode.Encrypt
            ? "Encrypting mode"
            : "Decrypting mode");
        Console.WriteLine();
    }

    public void WaitForEnterToReturnToMenu()
    {
        Console.WriteLine();
        Console.Write("Press Enter to return to the menu...");
        Console.ReadLine();
    }

    private static string ReadRequiredLine()
    {
        string? value = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("A value is required.");
        }

        return value.Trim().Trim('"');
    }

    private static bool ReadYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? answer = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (answer is "y" or "yes")
            {
                return true;
            }

            if (answer is "n" or "no")
            {
                return false;
            }

            Console.WriteLine("Please enter y or n.");
        }
    }

    private static string CreateOutputFilePath(string inputFile, OperationMode mode)
    {
        return mode == OperationMode.Encrypt
            ? $"{inputFile}{EncryptedFileSuffix}"
            : CreateDecryptedOutputFilePath(inputFile);
    }

    private static string CreateDecryptedOutputFilePath(string inputFile)
    {
        if (inputFile.EndsWith(EncryptedFileSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return inputFile[..^EncryptedFileSuffix.Length];
        }

        string? directory = Path.GetDirectoryName(inputFile);
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(inputFile);
        string extension = Path.GetExtension(inputFile);
        string decryptedFileName = $"{fileNameWithoutExtension}_decrypted{extension}";

        return string.IsNullOrEmpty(directory)
            ? decryptedFileName
            : Path.Combine(directory, decryptedFileName);
    }

    private static string ReadPassword()
    {
        var password = new StringBuilder();

        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password.Remove(password.Length - 1, 1);
                    Console.Write("\b \b");
                }

                continue;
            }

            password.Append(key.KeyChar);
            Console.Write("*");
        }

        Console.WriteLine();
        return password.ToString();
    }
}
