using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace KeySafe.Licensing;
/// <summary>
/// Validates licenses by checking signatures and communicating with a license server.
/// </summary>
public class LicenseValidator : IDisposable
{
    private bool _disposed;
    private readonly string _url;
    private readonly string _endpoint;
    private readonly string _licensePath;
    private readonly string _publicKeyPath;
    private readonly RestClient _client;
    private readonly RSA _rsa;
    /// <summary>
    /// Initializes a new instance of the <see cref="LicenseValidator"/> class.
    /// </summary>
    /// <param name="url">The base URL of the license server.</param>
    /// <param name="endpoint">The endpoint for license requests (e.g., "api/license").</param>
    /// <param name="licensePath">The path to the license JSON file.</param>
    /// <param name="publicKeyPath">The path to the public key PEM file.</param>
    /// <param name="baseDirectory">The base directory for resolving file paths. Defaults to the current directory if null.</param>
    /// <exception cref="FileNotFoundException">Thrown if the license or public key file does not exist or is in the wrong format.</exception>
    public LicenseValidator(string url, string endpoint, string licensePath, string publicKeyPath, string? baseDirectory = null)
    {
        var baseDir = baseDirectory ?? Directory.GetCurrentDirectory();

        _url = url;
        _endpoint = endpoint;
        var licenseFullPath = Path.Combine(baseDir, licensePath);
        if (!File.Exists(licenseFullPath) || Path.GetExtension(licensePath) != "json")
        {
            throw new FileNotFoundException("License file not found or in the wrong format", licenseFullPath);
        }
        var publicKeyFullPath = Path.Combine(baseDir, publicKeyPath);
        if (!File.Exists(publicKeyFullPath) || Path.GetExtension(publicKeyFullPath) != "pem")
        {
            throw new FileNotFoundException("public key file not found or in the wrong format", licenseFullPath);
        }
        _licensePath = licenseFullPath;
        _publicKeyPath = publicKeyFullPath;
        _client = new RestClient(_url);
        _rsa = RSA.Create();
    }


    private async Task<LicenseData?> GetLicense()
    {
        try
        {
            _rsa.ImportFromPem(await File.ReadAllTextAsync(_publicKeyPath));
            var licenseJson = await File.ReadAllTextAsync(_licensePath);
            var license = JsonConvert.DeserializeObject<License>(licenseJson);
            if (license == null)
            {
                return null;
            }
            var dataBytes = Convert.FromBase64String(license.Data);
            var signature = Convert.FromBase64String(license.Signature);

            var isValid = _rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            if (isValid)
            {
                var jsonData = Encoding.UTF8.GetString(dataBytes);
                var licenseData = JsonConvert.DeserializeObject<LicenseData>(jsonData);
                return licenseData;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
    private async Task<HttpStatusCode?> GenerateNewLicense(Guid id)
    {
        try
        {
            var request = new RestRequest($"{_endpoint}/{id}", Method.Get);
            var response = await _client.ExecuteAsync(request);
            if (response.IsSuccessful && !string.IsNullOrEmpty(response.Content))
            {
                try
                {
                    var newLicense = JsonConvert.DeserializeObject<License>(response.Content);
                    if (newLicense != null)
                    {
                        await File.WriteAllTextAsync(_licensePath, response.Content);
                    }
                }
                catch
                {
                    return null;
                }
            }
            return response.StatusCode;
        }
        catch
        {
            return null;
        }
    }
    /// <summary>
    /// Validates the license by checking its signature and server status, updating the global license state.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task ValidateLicense()
    {
        var license = await GetLicense();

        if (license == null)
        {
            GlobalLicenseState.UpdateState(false, null);
            return;
        }

        var status = await GenerateNewLicense(license.UserId);

        if (status == HttpStatusCode.OK && license.ExpiryDate >= DateTime.Now)
        {
            GlobalLicenseState.UpdateState(true, license.ExpiryDate);
        }
        else if (status == HttpStatusCode.Unauthorized || status == HttpStatusCode.NotFound)
        {
            GlobalLicenseState.UpdateState(false, null);
        }
        else if (license.BackupExpiryDate >= DateTime.Now)
        {
            GlobalLicenseState.UpdateState(true, license.ExpiryDate);
        }
        else
        {
            GlobalLicenseState.UpdateState(false, null);
        }
    }
    /// <summary>
    /// Disposes of the resources used by the validator.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _rsa?.Dispose();
            _client?.Dispose();
            _disposed = true;
        }
    }
}