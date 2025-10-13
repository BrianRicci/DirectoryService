using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTests : DirectoryBaseTests
{
    public CreateDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task CreateDepartment_With_Parent_Should_Succeed()
    {
        // arrange
        LocationId locationId = await CreateLocation();
        
        DepartmentId parentId = await CreateParentDepartmentWithLocation(locationId);

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение", 
                    "department", 
                    parentId.Value, 
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(
                    d => d.Id == new DepartmentId(result.Value),
                    cancellationToken);
            
            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
            
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }
    
    [Fact]
    public async Task CreateDepartment_Without_Parent_Should_Succeed()
    {
        // arrange
        LocationId locationId = await CreateLocation();

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "department",
                    null,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(
                    d => d.Id == new DepartmentId(result.Value),
                    cancellationToken);
            
            Assert.NotNull(department);
            Assert.Equal(department.Id.Value, result.Value);
            
            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_When_NameIsEmpty_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation();

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    " ",
                    "department",
                    null,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
    }
    
    [Fact]
    public async Task CreateDepartment_When_NameIsTooLong_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation();

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "A".PadRight(151, 'A'),
                    "department",
                    null,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task CreateDepartment_When_IdentifierIsEmpty_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation();

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    " ",
                    null,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
    }
    
    [Fact]
    public async Task CreateDepartment_When_IdentifierIsInvalid_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation();

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "department@",
                    null,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_When_ParentIdNonExistent_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation();
        DepartmentId parentId = new DepartmentId(Guid.NewGuid());

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "department",
                    parentId.Value,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task CreateDepartment_When_LocationIdIsEmpty_Should_Failed()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "department",
                    null,
                    []));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task CreateDepartment_When_LocationIdNonExistent_Should_Failed()
    {
        // arrange
        LocationId locationId = new LocationId(Guid.NewGuid());

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand(
                new CreateDepartmentRequest(
                    "Подразделение",
                    "department",
                    null,
                    [locationId.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }

    private async Task<LocationId> CreateLocation()
    {
        return await ExecuteInDb(async dbContext =>
        {
            LocationId locationId = new LocationId(Guid.NewGuid());
            
            var location = Location.Create(
                locationId,
                LocationName.Create("Локация").Value,
                LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
                LocationTimezone.Create("Europe/Moscow").Value).Value;
            
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
    
    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentHandler>();
        
        return await action(sut);
    }
}