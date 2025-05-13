using Newtonsoft.Json;
using RestSharp;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace KeySafe.Licensing;

public class LicenseValidator
{

    private readonly string _url;
    private readonly string _endpoint;
    private readonly string _licensePath;
    private readonly string _publicKeyPath;

    public LicenseValidator(string url, string endpoint,string licensePath,string publicKeyPath)
    {
        _url = url;
        _endpoint = endpoint;
        var licenseFullPath = Path.Combine(Directory.GetCurrentDirectory(), licensePath);
        if (!File.Exists(licenseFullPath) || Path.GetExtension(licensePath) != "json")
        {
            throw new FileNotFoundException("License file not found or in the wrong format", licenseFullPath);
        }
        var publicKeyFullPath = Path.Combine(Directory.GetCurrentDirectory(), publicKeyPath);
        if (!File.Exists(publicKeyFullPath) || Path.GetExtension(publicKeyFullPath) != "pem")
        {
            throw new FileNotFoundException("public key file not found or in the wrong format", licenseFullPath);
        }
        _licensePath = licenseFullPath;
        _publicKeyPath = publicKeyFullPath;
    }


    private async Task<LicenseData?> GetLicense()
    {
        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(await File.ReadAllTextAsync(_publicKeyPath));
            var licenseJson = await File.ReadAllTextAsync(_licensePath);
            var license = JsonConvert.DeserializeObject<License>(licenseJson);
            if (license == null)
            {
                return null;
            }
            var dataBytes = Convert.FromBase64String(license.Data);
            var signature = Convert.FromBase64String(license.Signature);

            var isValid = rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

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
            var client = new RestClient(_url);
            var request = new RestRequest($"{_endpoint}/{id}", Method.Get);
            request.AddParameter("id", id, ParameterType.UrlSegment);
            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "license.json");
                await File.WriteAllTextAsync(filePath, response.Content);
            }
            return response.StatusCode;
        }
        catch
        {
            return null;
        }
    }
    public async Task ValidateLicense()
    {
        var license = await GetLicense();

        if (license == null)
        {
            GlobalLicenseState.Current.IsActive = false;
            GlobalLicenseState.Current.ExpirationDate = null;
            return;
        }

        var status = await GenerateNewLicense(license.UserId);

        if (status == HttpStatusCode.OK)
        {
            GlobalLicenseState.Current.IsActive = true;
            GlobalLicenseState.Current.ExpirationDate = license.ExpiryDate;
        }
        else if (status == HttpStatusCode.Unauthorized || status == HttpStatusCode.NotFound)
        {
            GlobalLicenseState.Current.IsActive = false;
            GlobalLicenseState.Current.ExpirationDate = null;
        }
        else if (license.BackupExpiryDate >= DateTime.Now)
        {
            GlobalLicenseState.Current.IsActive = true;
            GlobalLicenseState.Current.ExpirationDate = license.ExpiryDate;
        }
        else
        {
            GlobalLicenseState.Current.IsActive = false;
            GlobalLicenseState.Current.ExpirationDate = null;
        }
    }
}