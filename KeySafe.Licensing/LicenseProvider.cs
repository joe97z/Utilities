using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace KeySafe.Licensing;

public class LicenseProvider
{
    private readonly int _days;
    private readonly string _privateKeyPath;

    public LicenseProvider(int days,string privateKeyPath)
    {
        var privateKeyFullPath = Path.Combine(Directory.GetCurrentDirectory(), privateKeyPath);
        if (!File.Exists(privateKeyFullPath) || Path.GetExtension(privateKeyFullPath) != "pem")
        {
            throw new FileNotFoundException("private key file not found or in the wrong format", privateKeyFullPath);
        }
        _days = days;
        _privateKeyPath = privateKeyFullPath;
    }

    public async Task<byte[]> GenerateLicense(Guid id, DateTime expiryDate)
    {
        var licenseData = new LicenseData
        {
            UserId = id,
            ExpiryDate = expiryDate,
            BackupExpiryDate = DateTime.Now.AddDays(_days),
        };
        var privateKeyPath = Path.Combine(Directory.GetCurrentDirectory(), _privateKeyPath);
        using RSA rsa = RSA.Create();
        rsa.ImportFromPem(await File.ReadAllTextAsync(privateKeyPath));
        var jsonData = JsonConvert.SerializeObject(licenseData);

        var dataBytes = Encoding.UTF8.GetBytes(jsonData);
        var signature = rsa.SignData(dataBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var license = new
        {
            data = Convert.ToBase64String(dataBytes),
            signature = Convert.ToBase64String(signature)
        };
        var json = JsonConvert.SerializeObject(license, Formatting.Indented);
        var byteArray = Encoding.UTF8.GetBytes(json);
        return byteArray;
    }

    public static (string privateKeyPem, string publicKeyPem) GenerateRsaKeyPair(int keySize = 2048)
    {
        using var rsa = RSA.Create(keySize);

        var privateKeyBytes = rsa.ExportPkcs8PrivateKey();
        var privateKeyPem = PemEncode("PRIVATE KEY", privateKeyBytes);

        var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();
        var publicKeyPem = PemEncode("PUBLIC KEY", publicKeyBytes);

        File.WriteAllText("private.pem", privateKeyPem);
        File.WriteAllText("public.pem", publicKeyPem);

        return (privateKeyPem, publicKeyPem);
    }
    private static string PemEncode(string label, byte[] data)
    {
        var base64 = Convert.ToBase64String(data, Base64FormattingOptions.InsertLineBreaks);
        return $"-----BEGIN {label}-----\n{base64}\n-----END {label}-----";
    }
}
