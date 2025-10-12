using DirectoryService.Application.Departments.MoveDepartment;
using DirectoryService.Contracts.Departments;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class MoveDepartmentTests : DirectoryBaseTests
{
    public MoveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task UpdateDepartment_When_ParentIdIsNotNull_Should_Succeed()
    {
        // arrange
        LocationId locationId = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);

        DepartmentIdentifier firstRootDepartmentIdentifier = DepartmentIdentifier.Create("department1").Value;
        Department firstRootDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            firstRootDepartmentIdentifier,
            DepartmentPath.CreateParent(firstRootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier secondRootDepartmentIdentifier = DepartmentIdentifier.Create("department2").Value;
        Department secondRootDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение 2").Value,
            secondRootDepartmentIdentifier,
            DepartmentPath.CreateParent(secondRootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier devDepartmentIdentifier = DepartmentIdentifier.Create("dev").Value;
        Department devDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел разработки").Value,
            devDepartmentIdentifier,
            firstRootDepartment.Path.CreateChild(devDepartmentIdentifier).Value,
            1,
            new List<DepartmentLocation>(),
            firstRootDepartment.Id);
        
        DepartmentIdentifier backendDepartmentIdentifier = DepartmentIdentifier.Create("backend").Value;
        Department backendDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел backend").Value,
            backendDepartmentIdentifier,
            devDepartment.Path.CreateChild(backendDepartmentIdentifier).Value,
            2,
            new List<DepartmentLocation>(),
            devDepartment.Id);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(
                devDepartment.Id.Value,
                new MoveDepartmentRequest(secondRootDepartment.Id.Value));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Identifier == backendDepartmentIdentifier, cancellationToken);
            
            Assert.Equal("department2.dev.backend", department.Path.Value);
            Assert.Equal((short)2, department.Depth);
            Assert.True(result.IsSuccess);
        });
    }
    
    [Fact]
    public async Task UpdateDepartment_When_ParentIdIsNull_Should_Succeed()
    {
        // arrange
        LocationId locationId = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);

        DepartmentIdentifier rootDepartmentIdentifier = DepartmentIdentifier.Create("department1").Value;
        Department rootDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            rootDepartmentIdentifier,
            DepartmentPath.CreateParent(rootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier devDepartmentIdentifier = DepartmentIdentifier.Create("dev").Value;
        Department devDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел разработки").Value,
            devDepartmentIdentifier,
            rootDepartment.Path.CreateChild(devDepartmentIdentifier).Value,
            1,
            new List<DepartmentLocation>(),
            rootDepartment.Id);
        
        DepartmentIdentifier backendDepartmentIdentifier = DepartmentIdentifier.Create("backend").Value;
        Department backendDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел backend").Value,
            backendDepartmentIdentifier,
            devDepartment.Path.CreateChild(backendDepartmentIdentifier).Value,
            2,
            new List<DepartmentLocation>(),
            devDepartment.Id);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(
                devDepartment.Id.Value,
                new MoveDepartmentRequest(null));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var department = await dbContext.Departments
                .FirstAsync(d => d.Identifier == backendDepartmentIdentifier, cancellationToken);
            
            Assert.Equal("dev.backend", department.Path.Value);
            Assert.Equal((short)1, department.Depth);
            Assert.True(result.IsSuccess);
        });
    }

    [Fact]
    public async Task UpdateDepartment_When_DepartmentIdIsNonExistent_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);

        DepartmentIdentifier rootDepartmentIdentifier = DepartmentIdentifier.Create("department1").Value;
        Department rootDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            rootDepartmentIdentifier,
            DepartmentPath.CreateParent(rootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier devDepartmentIdentifier = DepartmentIdentifier.Create("dev").Value;
        Department devDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел разработки").Value,
            devDepartmentIdentifier,
            rootDepartment.Path.CreateChild(devDepartmentIdentifier).Value,
            1,
            new List<DepartmentLocation>(),
            rootDepartment.Id);
        
        DepartmentIdentifier backendDepartmentIdentifier = DepartmentIdentifier.Create("backend").Value;
        Department backendDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел backend").Value,
            backendDepartmentIdentifier,
            devDepartment.Path.CreateChild(backendDepartmentIdentifier).Value,
            2,
            new List<DepartmentLocation>(),
            devDepartment.Id);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(
                Guid.NewGuid(),
                new MoveDepartmentRequest(null));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
    }
    
    [Fact]
    public async Task UpdateDepartment_When_ParentIdIsNonExistent_Should_Failed()
    {
        // arrange
        LocationId locationId = await CreateLocation(
            LocationName.Create("Локация 1").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);

        DepartmentIdentifier rootDepartmentIdentifier = DepartmentIdentifier.Create("department1").Value;
        Department rootDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            rootDepartmentIdentifier,
            DepartmentPath.CreateParent(rootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier devDepartmentIdentifier = DepartmentIdentifier.Create("dev").Value;
        Department devDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел разработки").Value,
            devDepartmentIdentifier,
            rootDepartment.Path.CreateChild(devDepartmentIdentifier).Value,
            1,
            new List<DepartmentLocation>(),
            rootDepartment.Id);
        
        DepartmentIdentifier backendDepartmentIdentifier = DepartmentIdentifier.Create("backend").Value;
        Department backendDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел backend").Value,
            backendDepartmentIdentifier,
            devDepartment.Path.CreateChild(backendDepartmentIdentifier).Value,
            2,
            new List<DepartmentLocation>(),
            devDepartment.Id);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new MoveDepartmentCommand(
                devDepartment.Id.Value,
                new MoveDepartmentRequest(Guid.NewGuid()));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
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
    
    private async Task<T> ExecuteHandler<T>(Func<MoveDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<MoveDepartmentHandler>();
        
        return await action(sut);
    }
}