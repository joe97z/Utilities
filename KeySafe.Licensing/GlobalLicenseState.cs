namespace KeySafe.Licensing;

internal class GlobalLicenseState
{
    public static LicenseState Current { get; } = new();

}
