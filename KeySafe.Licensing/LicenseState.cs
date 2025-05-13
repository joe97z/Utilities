namespace KeySafe.Licensing;

public class LicenseState
{
    public bool IsActive { get; set; }
    public DateTime? ExpirationDate { get; set; }
}
