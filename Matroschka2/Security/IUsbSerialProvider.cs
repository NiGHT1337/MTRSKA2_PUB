namespace Matroschka2.Security;

internal interface IUsbSerialProvider
{
    IEnumerable<string> GetUsbSerialNumbers();
}
