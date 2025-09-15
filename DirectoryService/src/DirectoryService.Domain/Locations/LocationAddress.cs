using CSharpFunctionalExtensions;

namespace DirectoryService.Domain.Locations;

public record LocationAddress
{
    public string Country { get; }
    
    public string Region { get; }
    
    public string City { get; }
    
    public string Street { get; }
    
    public string House { get; }
    
    private LocationAddress(string country, string region, string city, string street, string house)
    {
        Country = country;
        Region = region;
        City = city;
        Street = street;
        House = house;
    }
    
    public static Result<LocationAddress> Create(
        string country,
        string region,
        string city,
        string street,
        string house)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return Result.Failure<LocationAddress>("Country can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException("Region can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("City can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Street can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(house))
        {
            throw new ArgumentException("House can't be empty or null");
        }
        
        country = country.Trim();
        region = region.Trim();
        city = city.Trim();
        street = street.Trim();
        house = house.Trim();

        return new LocationAddress(country, region, city, street, house);
    }
}