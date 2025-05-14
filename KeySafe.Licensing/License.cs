namespace KeySafe.Licensing;

/// <summary>
/// Represents a license with serialized data and its cryptographic signature.
/// </summary>
public class License
{
    /// <summary>
    /// Gets or sets the Base64-encoded license data.
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Base64-encoded signature of the license data.
    /// </summary>
    public string Signature { get; set; } = string.Empty;
}