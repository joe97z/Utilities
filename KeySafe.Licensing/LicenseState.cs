namespace KeySafe.Licensing;

/// <summary>
/// Represents the state of a license, including its active status and expiration date.
/// </summary>
/// <param name="isActive">Indicates whether the license is active.</param>
/// <param name="expirationDate">The expiration date of the license, or null if not applicable.</param>
public class LicenseState(bool isActive, DateTime? expirationDate)
{
    /// <summary>
    /// Gets a value indicating whether the license is active.
    /// </summary>
    public bool IsActive { get; } = isActive;

    /// <summary>
    /// Gets the expiration date of the license, or null if not applicable.
    /// </summary>
    public DateTime? ExpirationDate { get; } = expirationDate;
}