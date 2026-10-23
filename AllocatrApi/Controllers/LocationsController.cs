using AllocatrApi.Data;
using AllocatrApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AllocatrApi.Controllers;

[ApiController]
[Route("api/locations")]
[AllowAnonymous]
public class LocationsController : ControllerBase
{
    private readonly AllocatrDbContext _dbContext;

    public LocationsController(AllocatrDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /* --------------------------------------------------------
     * COUNTRIES
     * All countries for account residence; operating countries
     * only when choosing an Allocat's operating location.
     * -------------------------------------------------------- */

    [HttpGet("countries")]
    public async Task<ActionResult<List<LocationCountryDto>>> GetCountries(
        [FromQuery] bool operatingOnly = false,
        CancellationToken cancellationToken = default
    )
    {
        var query = _dbContext.Countries.AsNoTracking();

        if (operatingOnly)
        {
            query = query.Where(country => country.SupportsAllocatOperations);
        }

        var countries = await query
            .OrderBy(country => country.Name)
            .Select(country => new LocationCountryDto(
                country.Code,
                country.Name,
                country.SupportsAllocatOperations
            ))
            .ToListAsync(cancellationToken);

        return Ok(countries);
    }

    /* --------------------------------------------------------
     * OPERATING CITIES
     * Residence cities remain free text on the account profile.
     * -------------------------------------------------------- */

    [HttpGet("countries/{countryCode}/cities")]
    public async Task<ActionResult<List<LocationCityDto>>> GetCities(
        string countryCode,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedCode = countryCode.Trim().ToUpperInvariant();

        if (normalizedCode.Length != 2 ||
            normalizedCode.Any(character => character < 'A' || character > 'Z'))
        {
            return BadRequest(new
            {
                message = "Country must use a two-letter code such as GB, ZA or ZW."
            });
        }

        var country = await _dbContext.Countries
            .AsNoTracking()
            .Where(country => country.Code == normalizedCode)
            .Select(country => new LocationCountryDto(
                country.Code,
                country.Name,
                country.SupportsAllocatOperations
            ))
            .SingleOrDefaultAsync(cancellationToken);

        if (country == null)
        {
            return NotFound(new { message = "Country not found." });
        }

        // A residence country does not need a seeded city catalogue.
        // Unsupported operating countries return an empty list, not an error.
        if (!country.SupportsAllocatOperations)
        {
            return Ok(new List<LocationCityDto>());
        }

        var cities = await _dbContext.Cities
            .AsNoTracking()
            .Where(city => city.CountryCode == normalizedCode)
            .OrderBy(city => city.Name)
            .Select(city => new LocationCityDto(
                city.Id,
                city.Name,
                city.CountryCode
            ))
            .ToListAsync(cancellationToken);

        return Ok(cities);
    }
}
