using DirectoryService.Application.Departments.Command.UpdateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentTests : DirectoryBaseTests
{
    public UpdateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task UpdateDepartment_Should_Succeed()
    {
        // arrange
        LocationId locationIdInitial = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        DepartmentId departmentId = await CreateParentDepartmentWithLocation(locationIdInitial);
        
        LocationId locationIdAdded = await CreateLocation(
            LocationName.Create("Локация 2").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "2").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest([locationIdInitial.Value, locationIdAdded.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .Include(department => department.DepartmentLocations)
                .FirstAsync(d => d.Id == departmentId, cancellationToken);

            List<DepartmentLocation> expected = department.DepartmentLocations.ToList();
            List<DepartmentLocation> actual = result.Value;
            expected.Sort((a, b) => a.LocationId.Value.CompareTo(b.LocationId.Value));
            actual.Sort((a, b) => a.LocationId.Value.CompareTo(b.LocationId.Value));
            
            Assert.Equal(expected.Count, actual.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.Equal(expected[i].DepartmentId.Value, actual[i].DepartmentId.Value);
                Assert.Equal(expected[i].Id.Value, actual[i].Id.Value);
                Assert.Equal(expected[i].LocationId.Value, actual[i].LocationId.Value);
            }
            
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task UpdateDepartment_When_DepartmentIdNonExistent_Should_Failure()
    {
        // arrange
        LocationId locationIdInitial = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        DepartmentId departmentId = new DepartmentId(Guid.NewGuid());
        
        LocationId locationIdAdded = await CreateLocation(
            LocationName.Create("Локация 2").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "2").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest([locationIdInitial.Value, locationIdAdded.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateDepartment_When_LocationIdNonExistent_Should_Failure()
    {
        // arrange
        LocationId locationIdInitial = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        DepartmentId departmentId = await CreateParentDepartmentWithLocation(locationIdInitial);
        
        LocationId locationIdAdded = new LocationId(Guid.NewGuid());
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest([locationIdInitial.Value, locationIdAdded.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateDepartment_When_DepartmentIdIsEmpty_Should_Failure()
    {
        // arrange
        LocationId locationId = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                Guid.Empty, 
                new UpdateDepartmentLocationsRequest([locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdateDepartment_When_LocationIdIsEmpty_Should_Failure()
    {
        // arrange
        LocationId locationIdInitial = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        DepartmentId departmentId = await CreateParentDepartmentWithLocation(locationIdInitial);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationsCommand(
                departmentId.Value,
                new UpdateDepartmentLocationsRequest([]));
            
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
            LocationId locationId = new LocationId(Guid.NewGuid());
            
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
    
    // Чтобы не создавать одну и ту же локацию 2 раза
    private async Task<DepartmentId> CreateParentDepartmentWithLocation(LocationId locationId)
    {
        return await ExecuteInDb(async dbContext =>
        {
            DepartmentId departmentId = new DepartmentId(Guid.NewGuid());
            DepartmentIdentifier departmentIdentifier = DepartmentIdentifier.Create("department").Value;
            List<DepartmentLocation> departmentLocations = new List<DepartmentLocation>();
            departmentLocations.Add(new DepartmentLocation(departmentId, locationId));
            
            var department = Department.CreateParent(
                DepartmentName.Create("Родительское подразделение").Value,
                departmentIdentifier,
                DepartmentPath.CreateParent(departmentIdentifier).Value,
                0,
                departmentLocations,
                departmentId).Value;
            
            dbContext.Departments.Add(department);
            await dbContext.SaveChangesAsync();

            return departmentId;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentLocationsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationsHandler>();
        
        return await action(sut);
    }
}