using DirectoryService.Application.Locations.Command.Update;
using DirectoryService.Contracts.Locations;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class UpdateLocationTests : DirectoryBaseTests
{
    public UpdateLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task UpdateLocation_Should_Succeed()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Старое название").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    "Новое название",
                    new LocationAddressDto("Россия", "Санкт-Петербург", "Санкт-Петербург", "Невский", "10"),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .FirstAsync(l => l.Id == locationId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Новое название", location.Name.Value);
            Assert.Equal("Россия", location.Address.Country);
            Assert.Equal("Санкт-Петербург", location.Address.Region);
            Assert.Equal("Санкт-Петербург", location.Address.City);
            Assert.Equal("Невский", location.Address.Street);
            Assert.Equal("10", location.Address.House);
            Assert.Equal("Europe/Moscow", location.Timezone.Value);
            Assert.True(location.UpdatedAt > location.CreatedAt);
        });
    }
    
    [Fact]
    public async Task UpdateLocation_Should_UpdateTimezone()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис Москва").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    "Офис Владивосток",
                    new LocationAddressDto("Россия", "Приморский край", "Владивосток", "Океанский", "20"),
                    "Asia/Vladivostok"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .FirstAsync(l => l.Id == locationId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Asia/Vladivostok", location.Timezone.Value);
            Assert.Equal("Офис Владивосток", location.Name.Value);
            Assert.Equal("Владивосток", location.Address.City);
        });
    }
    
    [Fact]
    public async Task UpdateLocation_Should_UpdateOnlyAddress()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    "Офис",
                    new LocationAddressDto("Россия", "Московская область", "Химки", "Мира", "5"),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .FirstAsync(l => l.Id == locationId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Офис", location.Name.Value);
            Assert.Equal("Химки", location.Address.City);
            Assert.Equal("Мира", location.Address.Street);
            Assert.Equal("5", location.Address.House);
        });
    }

    [Fact]
    public async Task UpdateLocation_When_LocationIdNonExistent_Should_Failure()
    {
        // arrange
        var nonExistentLocationId = new LocationId(Guid.NewGuid());
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                nonExistentLocationId.Value,
                new UpdateLocationRequest(
                    "Новое название",
                    new LocationAddressDto("Россия", "Москва", "Москва", "Ленина", "1"),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateLocation_When_LocationIdIsEmpty_Should_Failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                Guid.Empty,
                new UpdateLocationRequest(
                    "Название",
                    new LocationAddressDto("Россия", "Москва", "Москва", "Ленина", "1"),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateLocation_When_NameIsEmpty_Should_Failure()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    string.Empty,
                    new LocationAddressDto("Россия", "Москва", "Москва", "Ленина", "1"),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateLocation_When_InvalidTimezone_Should_Failure()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    "Офис",
                    new LocationAddressDto("Россия", "Москва", "Москва", "Ленина", "1"),
                    "Invalid/Timezone"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateLocation_When_AddressFieldsEmpty_Should_Failure()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    "Офис",
                    new LocationAddressDto(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateLocation_When_DeletedLocation_Should_Failure()
    {
        // arrange
        var locationId = await CreateDeletedLocation(
            LocationName.Create("Удаленная локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateLocationCommand(
                locationId.Value,
                new UpdateLocationRequest(
                    "Новое название",
                    new LocationAddressDto("Россия", "Москва", "Москва", "Ленина", "2"),
                    "Europe/Moscow"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    private async Task<LocationId> CreateLocation(
        LocationName locationName,
        LocationAddress locationAddress,
        LocationTimezone locationTimezone)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            
            var location = Location.Create(
                locationId,
                locationName,
                locationAddress,
                locationTimezone).Value;
            
            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return locationId;
        });
    }
    
    private async Task<LocationId> CreateDeletedLocation(
        LocationName locationName,
        LocationAddress locationAddress,
        LocationTimezone locationTimezone)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var locationId = new LocationId(Guid.NewGuid());
            
            var location = Location.Create(
                locationId,
                locationName,
                locationAddress,
                locationTimezone).Value;
            
            location.SoftDelete();
            
            dbContext.Locations.Add(location);
            await dbContext.SaveChangesAsync();

            return locationId;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdateLocationHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<UpdateLocationHandler>();
        
        return await action(sut);
    }
}