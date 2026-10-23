namespace AllocatrApi.Models;

public class City
{
    public int Id { get; set; }
    public required string Name { get; set; } = null!;
    public required string CountryCode { get; set; } = null!;

    public Country Country { get; set; } = null!;
}
