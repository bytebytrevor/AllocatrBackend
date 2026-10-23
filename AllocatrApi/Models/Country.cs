namespace AllocatrApi.Models;

public class Country
{
    public required string Code { get; set; } = null!;
    public required string Name { get; set; } = null!;

    // A country can be a user's residence without supporting Allocat operations.
    public bool SupportsAllocatOperations { get; set; } = false;

    public ICollection<City> Cities { get; set; } = new List<City>();
}
