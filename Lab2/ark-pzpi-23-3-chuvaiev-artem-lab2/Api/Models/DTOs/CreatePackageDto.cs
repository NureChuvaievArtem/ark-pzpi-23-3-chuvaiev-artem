namespace Api.Models.DTOs;

public class CreatePackageDto
{
    public int Height { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }
    public int CategoryId { get; set; }
    public int UserId { get; set; }
}

