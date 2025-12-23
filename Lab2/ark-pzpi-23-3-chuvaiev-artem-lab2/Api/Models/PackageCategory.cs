namespace Api.Models;

public class PackageCategory : BaseEntity
{
    public string Name { get; set; }
    
    public bool IsFragile { get; set; }
}