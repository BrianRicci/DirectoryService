using DirectoryService.Application.Departments.Command.Delete;
using DirectoryService.Domain;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Positions;
using DirectoryService.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class DeleteDepartmentTests : DirectoryBaseTests
{
    public DeleteDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task DeleteDepartment_WhenLocationAndPositionAreDeleted_ReturnsSuccess()
    {
        // arrange
        LocationId beingDeletedLocationId = await CreateLocation(
            LocationName.Create("Удаляемая локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        LocationId notDeletedLocationId = await CreateLocation(
            LocationName.Create("Не удаляемая локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "2").Value,
            LocationTimezone.Create("Europe/Moscow").Value);

        DepartmentIdentifier firstRootDepartmentIdentifier = DepartmentIdentifier.Create("firstDepartment").Value;
        Department firstRootDepartment = await CreateDepartmentWithLocation(
            beingDeletedLocationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            firstRootDepartmentIdentifier,
            DepartmentPath.CreateParent(firstRootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier secondRootDepartmentIdentifier = DepartmentIdentifier.Create("secondDepartment").Value;
        Department secondRootDepartment = await CreateDepartmentWithLocation(
            notDeletedLocationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение 2").Value,
            secondRootDepartmentIdentifier,
            DepartmentPath.CreateParent(secondRootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier devDepartmentIdentifier = DepartmentIdentifier.Create("dev").Value;
        Department devDepartment = await CreateDepartmentWithLocation(
            notDeletedLocationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел разработки").Value,
            devDepartmentIdentifier,
            firstRootDepartment.Path.CreateChild(devDepartmentIdentifier).Value,
            1,
            new List<DepartmentLocation>(),
            firstRootDepartment.Id);
        
        DepartmentIdentifier backendDepartmentIdentifier = DepartmentIdentifier.Create("backend").Value;
        Department backendDepartment = await CreateDepartmentWithLocation(
            notDeletedLocationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел backend").Value,
            backendDepartmentIdentifier,
            devDepartment.Path.CreateChild(backendDepartmentIdentifier).Value,
            2,
            new List<DepartmentLocation>(),
            devDepartment.Id);
        
        PositionId beingDeletedPositionId = await CreatePosition(
            firstRootDepartment.Id,
            PositionName.Create("Удаляемая должность").Value,
            PositionDescription.Create("Описание должности 1").Value,
            new List<DepartmentPosition>());

        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new DeleteDepartmentCommand(firstRootDepartment.Id.Value);
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var rootDepartment = await dbContext.Departments
                .FirstAsync(d => d.Identifier == firstRootDepartmentIdentifier, cancellationToken);
            
            var lastDescendantDepartment = await dbContext.Departments
                .FirstAsync(d => d.Identifier == backendDepartmentIdentifier, cancellationToken);
            
            var location = await dbContext.Locations
                .FirstAsync(l => l.Id == beingDeletedLocationId, cancellationToken);
            
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == beingDeletedPositionId, cancellationToken);
            
            int departmentLocations = await dbContext.DepartmentLocations
                .Where(dl => dl.LocationId == beingDeletedLocationId && dl.DepartmentId == firstRootDepartment.Id)
                .CountAsync(cancellationToken);
            
            int departmentPositions = await dbContext.DepartmentPositions
                .Where(dp => dp.PositionId == beingDeletedPositionId && dp.DepartmentId == firstRootDepartment.Id)
                .CountAsync(cancellationToken);
            
            // department asserts
            Assert.False(rootDepartment.IsActive);
            Assert.NotNull(rootDepartment.DeletedAt);
            Assert.StartsWith(Constants.DELETED_PREFIX, rootDepartment.Path.Value);
            
            Assert.True(lastDescendantDepartment.IsActive);
            Assert.Null(lastDescendantDepartment.DeletedAt);
            Assert.StartsWith(Constants.DELETED_PREFIX, lastDescendantDepartment.Path.Value);
            
            // location asserts
            Assert.False(location.IsActive);
            Assert.NotNull(location.DeletedAt);
            
            // position asserts
            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);
            
            // department locations asserts
            Assert.Equal(1, departmentLocations);
            
            // department positions asserts
            Assert.Equal(1, departmentPositions);
            
            Assert.True(result.IsSuccess);
        });
    }
    
    [Fact]
    public async Task DeleteDepartment_WhenLocationAndPositionAreNotDeleted_ReturnsSuccess()
    {
        // arrange
        LocationId locationId = await CreateLocation(
            LocationName.Create("Локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        DepartmentIdentifier firstRootDepartmentIdentifier = DepartmentIdentifier.Create("firstDepartment").Value;
        Department firstRootDepartment = await CreateDepartmentWithLocation(
            locationId,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            firstRootDepartmentIdentifier,
            DepartmentPath.CreateParent(firstRootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier secondRootDepartmentIdentifier = DepartmentIdentifier.Create("secondDepartment").Value;
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
        
        PositionId positionId = await CreatePosition(
            new List<DepartmentId>() { firstRootDepartment.Id, secondRootDepartment.Id },
            PositionName.Create("Должность").Value,
            PositionDescription.Create("Описание должности 1").Value,
            new List<DepartmentPosition>());
        
        await ExecuteInDb(async dbContext => await dbContext.SaveChangesAsync());
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new DeleteDepartmentCommand(firstRootDepartment.Id.Value);
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var rootDepartment = await dbContext.Departments
                .FirstAsync(d => d.Identifier == firstRootDepartmentIdentifier, cancellationToken);
            
            var lastDescendantDepartment = await dbContext.Departments
                .FirstAsync(d => d.Identifier == backendDepartmentIdentifier, cancellationToken);
            
            var location = await dbContext.Locations
                .FirstAsync(l => l.Id == locationId, cancellationToken);
            
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);
            
            int departmentLocations = await dbContext.DepartmentLocations
                .CountAsync(cancellationToken);
            
            int departmentPositions = await dbContext.DepartmentPositions
                .CountAsync(cancellationToken);
            
            // department asserts
            Assert.False(rootDepartment.IsActive);
            Assert.NotNull(rootDepartment.DeletedAt);
            Assert.StartsWith(Constants.DELETED_PREFIX, rootDepartment.Path.Value);
            
            Assert.True(lastDescendantDepartment.IsActive);
            Assert.Null(lastDescendantDepartment.DeletedAt);
            Assert.StartsWith(Constants.DELETED_PREFIX, lastDescendantDepartment.Path.Value);
            
            // location asserts
            Assert.True(location.IsActive);
            Assert.Null(location.DeletedAt);
            
            // position asserts
            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
            
            // department locations asserts
            Assert.Equal(4, departmentLocations);
            
            // department positions asserts
            Assert.Equal(2, departmentPositions);
            
            Assert.True(result.IsSuccess);
        });
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
    
    private async Task<PositionId> CreatePosition(
        DepartmentId departmentId,
        PositionName positionName,
        PositionDescription positionDescription,
        List<DepartmentPosition> departmentPositions)
    {
        return await ExecuteInDb(async dbContext =>
        {
            PositionId positionId = new(Guid.NewGuid());
            
            departmentPositions.Add(new DepartmentPosition(departmentId, positionId));
            
            var position = Position.Create(
                positionId,
                positionName,
                positionDescription,
                departmentPositions).Value;
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<PositionId> CreatePosition(
        List<DepartmentId> departmentIds,
        PositionName positionName,
        PositionDescription positionDescription,
        List<DepartmentPosition> departmentPositions)
    {
        return await ExecuteInDb(async dbContext =>
        {
            PositionId positionId = new(Guid.NewGuid());

            departmentIds.ForEach(departmentId =>
                departmentPositions.Add(new DepartmentPosition(departmentId, positionId)));
            
            var position = Position.Create(
                positionId,
                positionName,
                positionDescription,
                departmentPositions).Value;
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
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
    
    private async Task<T> ExecuteHandler<T>(Func<DeleteDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<DeleteDepartmentHandler>();
        
        return await action(sut);
    }
}