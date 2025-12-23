namespace Api.Models.DTOs;

public class OptimalPlacementRequestDto
{
    public PackageDimensionsDto Package { get; set; }
    public List<PostBoxCapacityDto> PostBoxes { get; set; }
}

public class PackageDimensionsDto
{
    public int Height { get; set; }
    public int Width { get; set; }
    public int Depth { get; set; }
}

public class PostBoxCapacityDto
{
    public int Id { get; set; }
    public int MaxVolume { get; set; }
    public int CurrentUsage { get; set; }
}

