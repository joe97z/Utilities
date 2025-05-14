namespace KeySafe.Licensing;

/// <summary>
/// Contains the details of a license, including user ID and expiration dates.
/// </summary>
public class LicenseData
{
    /// <summary>
    /// Gets or sets the unique identifier of the user associated with the license.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the primary expiration date of the license.
    /// </summary>
    public DateTime ExpiryDate { get; set; }

    /// <summary>
    /// Gets or sets the backup expiration date, used when the primary license cannot be validated.
    /// </summary>
    public DateTime BackupExpiryDate { get; set; }
}