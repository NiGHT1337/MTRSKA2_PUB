# Matroschka2

Matroschka2 is a Windows console application for password-based file encryption and decryption. It combines Argon2id key derivation with AES-GCM authenticated encryption and an optional USB-device authorization check.

The name is inspired by matryoshka dolls: layered objects nested inside each other. In the same spirit, this tool combines multiple security layers, such as password-based encryption, authenticated file protection, and optional local device authorization.

## Features

- Encrypt and decrypt files from the console.
- Derive encryption keys with Argon2id and per-file salts.
- Store encrypted files in a versioned format with per-chunk AES-GCM nonces.
- Keep legacy decrypt support for files produced by the original format.
- Optional USB authorization by comparing attached USB serial hashes against local configuration.

## Local Setup

USB authorization is disabled by default so the app can be tested without special hardware. To enable it for your own local setup, create `Matroschka2/appsettings.local.json`:

```json
{
  "MATROSCHKA2_ALLOWED_USB_HASH": "your-base64-sha256-usb-serial-hash"
}
```

That file is ignored by Git. If no real hash is configured, the application skips USB authorization. The environment variable also works and takes priority when set:

```powershell
$env:MATROSCHKA2_ALLOWED_USB_HASH = "your-base64-sha256-usb-serial-hash"
dotnet run --project Matroschka2
```

To create the USB hash again, plug in the USB device and run:

```powershell
$serial = (Get-CimInstance Win32_DiskDrive | Where-Object InterfaceType -eq "USB" | Select-Object -First 1 -ExpandProperty SerialNumber).Trim()
$sha = [Security.Cryptography.SHA256]::Create()
[Convert]::ToBase64String($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($serial)))
```

## Structure

- `Crypto`: encryption, decryption, key derivation, random byte generation, and file format constants.
- `Security`: USB serial discovery, hashing, and authorization checks.
- `UI`: console prompts, password input, progress display, and headers.
- `Models`: small request and mode types shared by the application flow.
- `ConsoleApplication.cs`: orchestration between UI, security, and crypto services.

## Security Notes

Use this project at your own risk. It is a portfolio/demo application and should be reviewed carefully before protecting important or irreplaceable data with it.

Do not commit real USB serials, serial hashes, passwords, private keys, local config files, or encrypted test files that contain private data. If a secret was previously pushed to GitHub, treat it as exposed and replace it.
