using CSharpFunctionalExtensions;
using Shared.SharedKernel;

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
    
    public static Result<LocationAddress, Error> Create(
        string country,
        string region,
        string city,
        string street,
        string house)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            return GeneralErrors.ValueIsRequired("Country can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(region))
        {
            return GeneralErrors.ValueIsRequired("Region can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(city))
        {
            return GeneralErrors.ValueIsRequired("City can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(street))
        {
            return GeneralErrors.ValueIsRequired("Street can't be empty or null");
        }
        
        if (string.IsNullOrWhiteSpace(house))
        {
            return GeneralErrors.ValueIsRequired("House can't be empty or null");
        }
        
        country = country.Trim();
        region = region.Trim();
        city = city.Trim();
        street = street.Trim();
        house = house.Trim();

        return new LocationAddress(country, region, city, street, house);
    }
}