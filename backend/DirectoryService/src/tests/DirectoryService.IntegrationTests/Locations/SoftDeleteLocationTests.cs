using DirectoryService.Application.Locations.Command.Delete;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Locations;

public class SoftDeleteLocationTests : DirectoryBaseTests
{
    public SoftDeleteLocationTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task SoftDeleteLocation_Should_Succeed()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис для удаления").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteLocationCommand(locationId.Value);
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .IgnoreQueryFilters()
                .FirstAsync(l => l.Id == locationId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(locationId.Value, result.Value);
            Assert.False(location.IsActive);
            Assert.NotNull(location.DeletedAt);
            Assert.True(location.DeletedAt <= DateTime.UtcNow);
            Assert.True(location.UpdatedAt >= location.CreatedAt);
        });
    }
    
    [Fact]
    public async Task SoftDeleteLocation_Should_UpdateTimestamps()
    {
        // arrange
        var locationId = await CreateLocation(
            LocationName.Create("Офис").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        var createdAtBefore = await ExecuteInDb(async dbContext =>
        {
            var loc = await dbContext.Locations.FirstAsync(l => l.Id == locationId, cancellationToken);
            return loc.CreatedAt;
        });
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteLocationCommand(locationId.Value);
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var location = await dbContext.Locations
                .IgnoreQueryFilters()
                .FirstAsync(l => l.Id == locationId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(createdAtBefore, location.CreatedAt);
            Assert.True(location.UpdatedAt > location.CreatedAt);
            Assert.NotNull(location.DeletedAt);
        });
    }

    [Fact]
    public async Task SoftDeleteLocation_When_LocationIdNonExistent_Should_Failure()
    {
        // arrange
        var nonExistentLocationId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteLocationCommand(nonExistentLocationId);
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task SoftDeleteLocation_When_LocationIdIsEmpty_Should_Failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteLocationCommand(Guid.Empty);
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task SoftDeleteLocation_When_AlreadyDeleted_Should_Failure()
    {
        // arrange
        var locationId = await CreateDeletedLocation(
            LocationName.Create("Уже удаленная локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeleteLocationCommand(locationId.Value);
            
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
    
    private async Task<Department> CreateDepartmentWithLocation(
        LocationId locationId,
        DepartmentId departmentId,
        DepartmentName departmentName,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        short departmentDepth,
        List<DepartmentLocation> departmentLocations,
        DepartmentId? departmentParentId = null)
    {
        return await ExecuteInDb(async dbContext =>
        {
            departmentLocations.Add(new DepartmentLocation(departmentId, locationId));
            
            var department = departmentParentId is null
                ? Department.CreateParent(
                    departmentName,
                    departmentIdentifier,
                    departmentPath,
                    departmentDepth,
                    departmentLocations,
                    departmentId).Value
                : Department.CreateChild(
                    new DepartmentId(departmentParentId.Value),
                    departmentName,
                    departmentIdentifier,
                    departmentPath,
                    departmentDepth,
                    departmentLocations,
                    departmentId).Value;
            
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();

            return department;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<SoftDeleteLocationHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<SoftDeleteLocationHandler>();
        
        return await action(sut);
    }
}