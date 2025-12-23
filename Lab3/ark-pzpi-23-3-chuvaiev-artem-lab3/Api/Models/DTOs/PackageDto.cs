namespace Api.Models.DTOs;

public class PackageDto
{
    public int Id { get; set; }
    
    public int Height { get; set; }
    
    public int Width { get; set; }
    
    public int Depth { get; set; }
    
    public int PostBoxId { get; set; }
    
    public string CategoryName { get; set; }
    
    public string DeliveryStatusName { get; set; }
    
    public DateTimeOffset CreatedOn { get; set; }
}

