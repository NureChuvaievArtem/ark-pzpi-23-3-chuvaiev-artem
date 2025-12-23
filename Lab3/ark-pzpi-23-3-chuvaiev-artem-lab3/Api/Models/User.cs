namespace Api.Models;

public class User : BaseEntity
{
    public string EmailAddress { get; set; }
    
    public string? SerialNfcData { get; set; }
    
    public IEnumerable<UserRole> Roles { get; set; }
    
    public IEnumerable<Package> Packages { get; set; }
}