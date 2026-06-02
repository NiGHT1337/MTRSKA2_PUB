namespace Matroschka2.Security;

internal sealed class UsbAuthorizationService
{
    private readonly AllowedUsbHashProvider _allowedUsbHashProvider;
    private readonly IUsbSerialProvider _usbSerialProvider;
    private readonly IHashService _hashService;

    public UsbAuthorizationService(
        AllowedUsbHashProvider allowedUsbHashProvider,
        IUsbSerialProvider usbSerialProvider,
        IHashService hashService)
    {
        _allowedUsbHashProvider = allowedUsbHashProvider;
        _usbSerialProvider = usbSerialProvider;
        _hashService = hashService;
    }

    public AuthorizationResult CheckAuthorization()
    {
        try
        {
            string? allowedUsbHash = _allowedUsbHashProvider.GetAllowedHash();
            if (allowedUsbHash is null)
            {
                return new AuthorizationResult(
                    true,
                    "USB authorization is disabled because no local hash is configured.");
            }

            foreach (string serial in _usbSerialProvider.GetUsbSerialNumbers())
            {
                if (_hashService.Hash(serial) == allowedUsbHash)
                {
                    return new AuthorizationResult(true, "Authorized device detected.");
                }
            }
        }
        catch (Exception ex)
        {
            return new AuthorizationResult(false, $"USB authorization check failed: {ex.Message}");
        }

        return new AuthorizationResult(false, "Unauthorized device. Exiting...");
    }
}
