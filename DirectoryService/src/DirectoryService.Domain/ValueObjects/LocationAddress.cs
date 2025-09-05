namespace DirectoryService.Domain.ValueObjects;

public record LocationAddress
{
    public LocationAddress(string country, string region, string city, string street, string house)
    {
        if (string.IsNullOrWhiteSpace(country))
        {
            throw new ArgumentException("Invalid country");
        }
        
        if (string.IsNullOrWhiteSpace(region))
        {
            throw new ArgumentException("Invalid region");
        }
        
        if (string.IsNullOrWhiteSpace(city))
        {
            throw new ArgumentException("Invalid city");
        }
        
        if (string.IsNullOrWhiteSpace(street))
        {
            throw new ArgumentException("Invalid street");
        }
        
        if (string.IsNullOrWhiteSpace(house))
        {
            throw new ArgumentException("Invalid house");
        }
        
        Country = country;
        Region = region;
        City = city;
        Street = street;
        House = house;
    }
    
    public string Country { get; }
    
    public string Region { get; }
    
    public string City { get; }
    
    public string Street { get; }
    
    public string House { get; }
}