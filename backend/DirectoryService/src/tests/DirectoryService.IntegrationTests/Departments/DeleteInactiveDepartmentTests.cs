using DirectoryService.Application.Departments.Command.DeleteInactive;
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

public class DeleteInactiveDepartmentTests : DirectoryBaseTests
{
    public DeleteInactiveDepartmentTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task DeleteInactiveDepartment_Should_Succeed()
    {
        // arrange
        Location beingDeletedLocation = await CreateLocation(
            LocationName.Create("Удаляемая локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "1").Value,
            LocationTimezone.Create("Europe/Moscow").Value);
        
        Location notDeletedLocation = await CreateLocation(
            LocationName.Create("Не удаляемая локация").Value,
            LocationAddress.Create("Россия", "Москва", "Москва", "Ленина", "2").Value,
            LocationTimezone.Create("Europe/Moscow").Value);

        DepartmentIdentifier rootDepartmentIdentifier = DepartmentIdentifier.Create("rootDepartment").Value;
        Department rootDepartment = await CreateDepartmentWithLocation(
            notDeletedLocation.Id,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Родительское подразделение").Value,
            rootDepartmentIdentifier,
            DepartmentPath.CreateParent(rootDepartmentIdentifier).Value,
            0,
            new List<DepartmentLocation>());
        
        DepartmentIdentifier devDepartmentIdentifier = DepartmentIdentifier.Create("dev").Value;
        Department beingDeletedDevDepartment = await CreateDepartmentWithLocation(
            beingDeletedLocation.Id,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел разработки").Value,
            devDepartmentIdentifier,
            rootDepartment.Path.CreateChild(devDepartmentIdentifier).Value,
            1,
            new List<DepartmentLocation>(),
            rootDepartment.Id);
        
        DepartmentIdentifier backendDepartmentIdentifier = DepartmentIdentifier.Create("backend").Value;
        Department backendDepartment = await CreateDepartmentWithLocation(
            notDeletedLocation.Id,
            new DepartmentId(Guid.NewGuid()),
            DepartmentName.Create("Отдел backend").Value,
            backendDepartmentIdentifier,
            beingDeletedDevDepartment.Path.CreateChild(backendDepartmentIdentifier).Value,
            2,
            new List<DepartmentLocation>(),
            beingDeletedDevDepartment.Id);
        
        Position beingDeletedPosition = await CreatePosition(
            beingDeletedDevDepartment.Id,
            PositionName.Create("Удаляемая должность").Value,
            PositionDescription.Create("Описание должности 1").Value,
            new List<DepartmentPosition>());
        
        var monthAgo = DateTime.UtcNow.AddMonths(-1).AddDays(-1);
        
        beingDeletedDevDepartment.SetPrivateProperty("IsActive", false);
        beingDeletedDevDepartment.SetPrivateProperty("DeletedAt", monthAgo);
        beingDeletedDevDepartment.SetPrivateProperty("UpdatedAt", monthAgo);
        
        beingDeletedLocation.SetPrivateProperty("IsActive", false);
        beingDeletedLocation.SetPrivateProperty("DeletedAt", monthAgo);
        beingDeletedLocation.SetPrivateProperty("UpdatedAt", monthAgo);
        
        beingDeletedPosition.SetPrivateProperty("IsActive", false);
        beingDeletedPosition.SetPrivateProperty("DeletedAt", monthAgo);
        beingDeletedPosition.SetPrivateProperty("UpdatedAt", monthAgo);

        var cancellationToken = CancellationToken.None;

        await ExecuteInDb(async dbContext =>
        {
            dbContext.Entry(beingDeletedDevDepartment).State = EntityState.Modified;
            dbContext.Entry(beingDeletedLocation).State = EntityState.Modified;
            dbContext.Entry(beingDeletedPosition).State = EntityState.Modified;
    
            await dbContext.SaveChangesAsync(cancellationToken);
        });
        
        // act
        var result = await ExecuteHandler(sut => sut.Handle(cancellationToken));
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var lastDescendantDepartment = await dbContext.Departments
                .FirstAsync(d => d.Identifier == backendDepartmentIdentifier, cancellationToken);
            
            var devDepartment = await dbContext.Departments
                .FirstOrDefaultAsync(d => d.Identifier == devDepartmentIdentifier, cancellationToken);
            
            var location = await dbContext.Locations
                .FirstOrDefaultAsync(l => l.Id == beingDeletedLocation.Id, cancellationToken);
            
            var position = await dbContext.Positions
                .FirstOrDefaultAsync(p => p.Id == beingDeletedPosition.Id, cancellationToken);
            
            int departmentLocations = await dbContext.DepartmentLocations
                .Where(dl => dl.LocationId == beingDeletedLocation.Id && dl.DepartmentId == beingDeletedDevDepartment.Id)
                .CountAsync(cancellationToken);
            
            int departmentPositions = await dbContext.DepartmentPositions
                .Where(dp => dp.PositionId == beingDeletedPosition.Id && dp.DepartmentId == beingDeletedDevDepartment.Id)
                .CountAsync(cancellationToken);
            
            // department asserts
            Assert.Null(devDepartment);
            
            Assert.True(lastDescendantDepartment.IsActive);
            Assert.Null(lastDescendantDepartment.DeletedAt);
            Assert.Equal(
                rootDepartmentIdentifier.Value + Constants.SEPARATOR + backendDepartmentIdentifier.Value, 
                lastDescendantDepartment.Path.Value);
            
            // location asserts
            Assert.Null(location);
            
            // position asserts
            Assert.Null(position);
            
            // department locations asserts
            Assert.Equal(0, departmentLocations);
            
            // department positions asserts
            Assert.Equal(0, departmentPositions);
            
            Assert.True(result.IsSuccess);
        });
    }
    
    private async Task<Location> CreateLocation(
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

            return location;
        });
    }
    
    private async Task<Position> CreatePosition(
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

            return position;
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
    
    private async Task<T> ExecuteHandler<T>(Func<DeleteInactiveHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<DeleteInactiveHandler>();
        
        return await action(sut);
    }
}