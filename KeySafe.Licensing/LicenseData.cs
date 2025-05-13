namespace KeySafe.Licensing;

public class LicenseData
{
    public Guid UserId { get; set; }
    public DateTime ExpiryDate { get; set; }
    public DateTime BackupExpiryDate { get; set; }
}
