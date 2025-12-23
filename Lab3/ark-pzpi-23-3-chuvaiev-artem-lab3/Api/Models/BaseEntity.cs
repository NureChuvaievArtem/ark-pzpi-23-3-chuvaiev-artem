namespace Api.Models;

public class BaseEntity
{
    public int Id { get; set; }
    
    public DateTimeOffset CreatedOn { get; set; }
    
    public DateTimeOffset LastModifiedOn { get; set; }
}