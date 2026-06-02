namespace Matroschka2.Models;

internal sealed record CryptoOperationRequest(
    OperationMode Mode,
    string InputFile,
    string OutputFile,
    string Password);
