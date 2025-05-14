# KeySafe.Licensing

**KeySafe.Licensing** is a .NET library for generating and validating RSA-signed software licenses. It provides a secure and flexible way to manage license states, generate cryptographically signed license files, and validate them against a server or locally with a backup expiration mechanism. The library is designed for thread-safety and ease of integration into .NET applications.

## Features

- **RSA Key Pair Generation**: Create secure RSA key pairs (2048-bit or higher) for signing and verifying licenses.
- **License Generation**: Generate JSON-based licenses with user-specific data and RSA signatures.
- **License Validation**: Verify license integrity using public keys and check license status via a server or backup expiration dates.
- **Thread-Safe License State Management**: Manage global license state with thread-safe operations.
- **Extensible**: Supports custom license servers and file-based license storage.

## Installation

Install the `KeySafe.Licensing` package via NuGet:

```bash
dotnet add package KeySafe.Licensing
```

Or, using the Package Manager Console:

```powershell
Install-Package KeySafe.Licensing
```

### Prerequisites

- **.NET Framework**: .NET 9.
- **File System Access**: Ensure the application has read/write permissions for license and key files.
- **License Server** (optional): A server implementing the license endpoint for validation (e.g., `GET /api/license/{userId}`).

## Getting Started

### 1. Generate RSA Key Pair

Before generating licenses, create an RSA key pair for signing and verification.

```csharp
using KeySafe.Licensing;

try
{
    var (privateKeyPem, publicKeyPem) = LicenseProvider.GenerateRsaKeyPair(
        keySize: 2048,
        publicKeyPath: "public.pem",
        privateKeyPath: "private.pem"
    );
    Console.WriteLine("RSA key pair generated successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating key pair: {ex.Message}");
}
```

This creates `public.pem` and `private.pem` files in the specified paths.

### 2. Generate a License

Use the `LicenseProvider` to generate a signed license for a user.

```csharp
using KeySafe.Licensing;

try
{
    var provider = new LicenseProvider(
        days: 30, // Backup expiration period
        privateKeyPath: "private.pem",
        baseDirectory: Directory.GetCurrentDirectory()
    );

    var userId = Guid.NewGuid();
    var expiryDate = DateTime.Now.AddDays(90);
    var licenseBytes = await provider.GenerateLicense(userId, expiryDate);

    File.WriteAllBytes("license.json", licenseBytes);
    Console.WriteLine("License generated and saved to license.json.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error generating license: {ex.Message}");
}
```

This generates a `license.json` file containing the signed license data.

### 3. Validate a License

Use the `LicenseValidator` to verify a license and update the global license state.

```csharp
using KeySafe.Licensing;

try
{
    using var validator = new LicenseValidator(
        url: "https://your-license-server.com",
        endpoint: "api/license",
        licensePath: "license.json",
        publicKeyPath: "public.pem",
        baseDirectory: Directory.GetCurrentDirectory()
    );

    await validator.ValidateLicense();
    var state = GlobalLicenseState.Current;

    if (state?.IsActive == true)
    {
        Console.WriteLine($"License is active. Expires: {state.ExpirationDate}");
    }
    else
    {
        Console.WriteLine("License is invalid or expired.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error validating license: {ex.Message}");
}
```

The validator checks the license signature, queries the server, and falls back to the backup expiration date if needed.

## Configuration

### File Paths

- **Private Key**: A PEM file (e.g., `private.pem`) containing the RSA private key for signing licenses.
- **Public Key**: A PEM file (e.g., `public.pem`) containing the RSA public key for verifying licenses.
- **License File**: A JSON file (e.g., `license.json`) containing the license data and signature.

Ensure these files are in a secure location with appropriate permissions.

### License Server

The `LicenseValidator` requires a server endpoint (e.g., `https://your-license-server.com/api/license/{userId}`) that returns a JSON license object. The server should respond with:

- **200 OK**: A valid license JSON object.
- **401 Unauthorized** or **404 Not Found**: Indicates an invalid or non-existent license.

If no server is available, the validator uses the backup expiration date.

## Usage Notes

- **Thread Safety**: The `GlobalLicenseState` class is thread-safe for reading and updating license states.
- **Disposal**: Always dispose of the `LicenseValidator` instance to release RSA and HTTP client resources.
- **Error Handling**: Wrap operations in try-catch blocks to handle file, cryptographic, or network errors.
- **Security**: Store private keys securely and restrict access to license files.

## Troubleshooting

- **FileNotFoundException**: Ensure the specified key or license files exist and have the correct extensions (`.pem` for keys, `.json` for licenses).
- **InvalidOperationException**: Check if key files already exist when generating keys or if the license generation fails due to invalid private key data.
- **Network Issues**: Verify the license server URL and endpoint are correct and accessible.
- **Signature Verification Failed**: Ensure the public key matches the private key used to sign the license.


## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.
