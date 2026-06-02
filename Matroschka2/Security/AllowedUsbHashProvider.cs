using System.Text.Json;

namespace Matroschka2.Security;

internal sealed class AllowedUsbHashProvider
{
    public const string EnvironmentVariableName = "MATROSCHKA2_ALLOWED_USB_HASH";
    private const string LocalConfigFileName = "appsettings.local.json";

    public string? GetAllowedHash()
    {
        string? environmentValue = Normalize(Environment.GetEnvironmentVariable(EnvironmentVariableName));
        if (environmentValue is not null)
        {
            return environmentValue;
        }

        foreach (string configPath in GetLocalConfigPaths())
        {
            string? configValue = ReadFromLocalConfig(configPath);
            if (configValue is not null)
            {
                return configValue;
            }
        }

        return null;
    }

    private static IEnumerable<string> GetLocalConfigPaths()
    {
        yield return Path.Combine(Environment.CurrentDirectory, LocalConfigFileName);
        yield return Path.Combine(Environment.CurrentDirectory, "Matroschka2", LocalConfigFileName);
        yield return Path.Combine(AppContext.BaseDirectory, LocalConfigFileName);
    }

    private static string? ReadFromLocalConfig(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        using FileStream stream = File.OpenRead(path);
        using JsonDocument document = JsonDocument.Parse(stream);

        if (!document.RootElement.TryGetProperty(EnvironmentVariableName, out JsonElement valueElement))
        {
            return null;
        }

        return valueElement.ValueKind == JsonValueKind.String
            ? Normalize(valueElement.GetString())
            : null;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        string trimmed = value.Trim();
        return trimmed.StartsWith("replace-", StringComparison.OrdinalIgnoreCase)
            ? null
            : trimmed;
    }
}
