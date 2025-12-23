namespace Api.Models;

public class AppLog : BaseEntity
{
    public string Application { get; set; }
    
    public int? BoxId { get; set; }
    
    public string Message { get; set; }
}