using Matroschka2;
using Matroschka2.Crypto;
using Matroschka2.Security;
using Matroschka2.UI;

var ui = new ConsoleUI();
var application = new ConsoleApplication(
    ui,
    new UsbAuthorizationService(
        new AllowedUsbHashProvider(),
        new WindowsUsbSerialProvider(),
        new Sha256HashService()),
    new FileEncryptionService(
        new Argon2KeyDerivationService(),
        new SecureRandomByteGenerator()));

application.Run();
