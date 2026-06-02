using System.Management;

namespace Matroschka2.Security;

internal sealed class WindowsUsbSerialProvider : IUsbSerialProvider
{
    public IEnumerable<string> GetUsbSerialNumbers()
    {
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException("USB serial detection is only supported on Windows.");
        }

        using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive WHERE InterfaceType='USB'");

        foreach (ManagementObject usb in searcher.Get().Cast<ManagementObject>())
        {
            string? serial = usb["SerialNumber"]?.ToString()?.Trim();
            if (!string.IsNullOrWhiteSpace(serial))
            {
                yield return serial;
            }
        }
    }
}
