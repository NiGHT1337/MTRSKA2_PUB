using Matroschka2.Crypto;
using Matroschka2.Models;
using Matroschka2.Security;
using Matroschka2.UI;

namespace Matroschka2;

internal sealed class ConsoleApplication
{
    private readonly ConsoleUI _ui;
    private readonly UsbAuthorizationService _authorizationService;
    private readonly FileEncryptionService _fileEncryptionService;

    public ConsoleApplication(
        ConsoleUI ui,
        UsbAuthorizationService authorizationService,
        FileEncryptionService fileEncryptionService)
    {
        _ui = ui;
        _authorizationService = authorizationService;
        _fileEncryptionService = fileEncryptionService;
    }

    public void Run()
    {
        _ui.ShowHeader();
        _ui.WriteLine("Checking optional USB authorization...");

        AuthorizationResult authorization = _authorizationService.CheckAuthorization();
        if (!authorization.IsAuthorized)
        {
            _ui.ClearWithHeader();
            _ui.WriteLine(authorization.Message);
            return;
        }

        _ui.WriteLine(authorization.Message);
        _ui.WriteLine("Success!");

        while (true)
        {
            OperationMode? mode = _ui.ReadMode();
            if (mode is null)
            {
                _ui.WriteLine("Invalid mode. Use 1 for encrypt or 2 for decrypt.");
                _ui.WaitForEnterToReturnToMenu();
                _ui.ClearWithHeader();
                continue;
            }

            _ui.ClearWithHeader();
            _ui.WriteMode(mode.Value);

            try
            {
                CryptoOperationRequest request = _ui.ReadOperationRequest(mode.Value);
                _ui.ClearWithHeader();

                var progressReporter = new ConsoleProgressReporter();

                if (request.Mode == OperationMode.Encrypt)
                {
                    _fileEncryptionService.EncryptFile(
                        request.InputFile,
                        request.OutputFile,
                        request.Password,
                        progressReporter);
                }
                else
                {
                    _fileEncryptionService.DecryptFile(
                        request.InputFile,
                        request.OutputFile,
                        request.Password,
                        progressReporter);
                }
            }
            catch (Exception ex)
            {
                _ui.ShowError(ex.Message);
            }

            _ui.WaitForEnterToReturnToMenu();
            _ui.ClearWithHeader();
        }
    }
}
