namespace KeySafe.Licensing;


/// <summary>
/// Represents the global state of a license, tracking its active status and expiration date.
/// </summary>
public class GlobalLicenseState
{
    private static volatile LicenseState? _current = new(false, null);

    /// <summary>
    /// Gets the current license state.
    /// </summary>
    /// <value>The current <see cref="LicenseState"/> instance, or null if not set.</value>
    public static LicenseState? Current => _current;

    /// <summary>
    /// Updates the current license state in a thread-safe manner.
    /// </summary>
    /// <param name="isActive">Indicates whether the license is active.</param>
    /// <param name="expirationDate">The expiration date of the license, or null if not applicable.</param>
    public static void UpdateState(bool isActive, DateTime? expirationDate)
    {
        var newState = new LicenseState(isActive, expirationDate);
        Interlocked.Exchange(ref _current, newState);
    }
}