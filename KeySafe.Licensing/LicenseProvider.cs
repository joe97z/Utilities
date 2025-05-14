using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace KeySafe.Licensing;
/// <summary>
/// Provides functionality to generate RSA-signed licenses.
/// </summary>
public class LicenseProvider
{
    private readonly int _days;
    private readonly string _privateKeyPath;

    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseProvider"/> class.
    /// </summary>
    /// <param name="days">The number of days to set for the backup expiration date if shorter than the primary expiration.</param>
    /// <param name="privateKeyPath">The relative or absolute path to the private key PEM file.</param>
    /// <param name="baseDirectory">The base directory for resolving the private key path. Defaults to the current directory if null.</param>
    /// <exception cref="FileNotFoundException">Thrown if the private key file does not exist or is not a PEM file.</exception>
    public LicenseProvider(int days, string privateKeyPath, string? baseDirectory = null)
    {
        var baseDir = baseDirectory ?? Directory.GetCurrentDirectory();
        var privateKeyFullPath = Path.Combine(baseDir, privateKeyPath);
        if (!File.Exists(privateKeyFullPath) || Path.GetExtension(privateKeyFullPath) != ".pem")
        {
            throw new FileNotFoundException("Private key file not found or in the wrong format", privateKeyFullPath);
        }
        _days = days;
        _privateKeyPath = privateKeyFullPath;
    }

    /// <summary>
    /// Generates a signed license for a user with the specified ID and expiration date.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="expiryDate">The expiration date of the license.</param>
    /// <returns>A byte array containing the JSON-encoded license with data and signature.</returns>
    /// <exception cref="InvalidOperationException">Thrown if license generation fails due to cryptographic or serialization errors.</exception>
    public async Task<byte[]> GenerateLicense(Guid id, DateTime expiryDate)
    {
        try
        {
            var licenseData = new LicenseData
            {
                UserId = id,
                ExpiryDate = expiryDate,
                BackupExpiryDate = DateTime.Now.AddDays(_days) < expiryDate ? DateTime.Now.AddDays(_days) : expiryDate
            };
            using RSA rsa = RSA.Create();
            rsa.ImportFromPem(await File.ReadAllTextAsync(_privateKeyPath));
            var jsonData = JsonConvert.SerializeObject(licenseData);

            var dataBytes = Encoding.UTF8.GetBytes(jsonData);
            var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            var license = new
            {
                data = Convert.ToBase64String(dataBytes),
                signature = Convert.ToBase64String(signature)
            };
            var json = JsonConvert.SerializeObject(license, Formatting.Indented);
            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate license.", ex);
        }
    }

    /// <summary>
    /// Generates an RSA key pair and saves them as PEM files.
    /// </summary>
    /// <param name="keySize">The size of the RSA key in bits (minimum 2048).</param>
    /// <param name="publicKeyPath">The path to save the public key PEM file.</param>
    /// <param name="privateKeyPath">The path to save the private key PEM file.</param>
    /// <returns>A tuple containing the private and public key PEM strings.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the key files already exist.</exception>
    /// <exception cref="ArgumentException">Thrown if the key size is less than 2048 bits.</exception>
    /// <exception cref="FileNotFoundException">Thrown if the key paths do not have a .pem extension.</exception>
    public static (string privateKeyPem, string publicKeyPem) GenerateRsaKeyPair(int keySize = 2048, string publicKeyPath = "public.pem", string privateKeyPath = "private.pem")
    {
        if (File.Exists(privateKeyPath) || File.Exists(publicKeyPath))
        {
            throw new InvalidOperationException("Key files already exist.");
        }
        if (keySize < 2048)
        {
            throw new ArgumentException("Key size must be at least 2048 bits.", nameof(keySize));
        }
        if (Path.GetExtension(privateKeyPath) != ".pem")
        {
            throw new FileNotFoundException("Private key should be a PEM file", privateKeyPath);
        }
        if (Path.GetExtension(publicKeyPath) != ".pem")
        {
            throw new FileNotFoundException("Public key should be a PEM file", publicKeyPath);
        }
        using var rsa = RSA.Create(keySize);

        var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
        var privateKeyPem = PemEncode("PRIVATE KEY", privateKeyBytes);

        var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
        var publicKeyPem = PemEncode("PUBLIC KEY", publicKeyBytes);

        File.WriteAllText(privateKeyPath, privateKeyPem);
        File.WriteAllText(publicKeyPath, publicKeyPem);

        return (privateKeyPem, publicKeyPem);
    }

    private static string PemEncode(string label, byte[] data)
    {
        var base64 = Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN {label}-----\n{base64}\n-----END {label}-----";
    }
}