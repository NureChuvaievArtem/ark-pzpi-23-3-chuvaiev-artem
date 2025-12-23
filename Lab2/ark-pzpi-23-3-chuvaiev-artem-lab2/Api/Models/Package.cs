namespace Api.Models;

public class Package : BaseEntity
{
    public int Height { get; set; }
    
    public int Width { get; set; }
    
    public int Depth { get; set; }
    
    public int PostBoxId { get; set; }
    
    public int UserId { get; set; }
    
    public User User { get; set; }
    
    public PackageCategory Category { get; set; }
    
    public DeliveryStatus DeliveryStatus { get; set; }
}