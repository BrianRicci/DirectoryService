using DirectoryService.Application.Positions.Command.UpdateDepartments;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Positions;

public class UpdatePositionDepartmentsTests : DirectoryBaseTests
{
    public UpdatePositionDepartmentsTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task UpdatePositionDepartments_Should_Succeed()
    {
        // arrange
        var (positionId, departmentIds) = await CreateTestData();
        var newDepartment = await CreateActiveDepartment("NEWDEPT", "Новый отдел");
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([newDepartment.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.DepartmentPositions)
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Single(position.DepartmentPositions);
            Assert.Equal(newDepartment.Id, position.DepartmentPositions.First().DepartmentId);
            Assert.Equal(positionId, position.DepartmentPositions.First().PositionId);
            Assert.True(position.UpdatedAt > position.CreatedAt);
        });
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_Should_AddMultipleDepartments()
    {
        // arrange
        var (positionId, _) = await CreateTestData();
        var department1 = await CreateActiveDepartment("DEV", "Отдел разработки");
        var department2 = await CreateActiveDepartment("QATEST", "Отдел тестирования");
        var department3 = await CreateActiveDepartment("ANALYTICS", "Отдел аналитики");
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([
                    department1.Id.Value,
                    department2.Id.Value,
                    department3.Id.Value
                ]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.DepartmentPositions)
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(3, result.Value.Count);
            Assert.Equal(3, position.DepartmentPositions.Count);
            
            var departmentIds = position.DepartmentPositions
                .Select(dp => dp.DepartmentId.Value)
                .ToHashSet();
            
            Assert.Contains(department1.Id.Value, departmentIds);
            Assert.Contains(department2.Id.Value, departmentIds);
            Assert.Contains(department3.Id.Value, departmentIds);
        });
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_Should_ReplaceExistingDepartments()
    {
        // arrange
        (PositionId positionId, List<Guid> existingDepartmentIds) = await CreateTestData();
        var newDepartment = await CreateActiveDepartment("NEWDEPT", "Новый отдел");
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([newDepartment.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.DepartmentPositions)
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Single(position.DepartmentPositions);
            Assert.Equal(newDepartment.Id, position.DepartmentPositions.First().DepartmentId);
            
            // Проверяем что старые связи удалены
            var oldConnections = await dbContext.DepartmentPositions
                .Where(dp => dp.PositionId == positionId && dp.DepartmentId != newDepartment.Id)
                .ToListAsync(cancellationToken);
            
            Assert.Empty(oldConnections);
        });
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_Should_ClearAllDepartments()
    {
        // arrange
        var (positionId, _) = await CreateTestData();
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_PositionIdNonExistent_Should_Failure()
    {
        // arrange
        var department = await CreateActiveDepartment("DEPT", "Отдел");
        var nonExistentPositionId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                nonExistentPositionId,
                new UpdatePositionDepartmentsRequest([department.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_PositionIdIsEmpty_Should_Failure()
    {
        // arrange
        var department = await CreateActiveDepartment("DEPT", "Отдел");
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                Guid.Empty,
                new UpdatePositionDepartmentsRequest([department.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_DepartmentIdsListIsEmpty_Should_Failure()
    {
        // arrange
        var positionId = await CreatePositionWithNoDepartments();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_DuplicateDepartmentIds_Should_Failure()
    {
        // arrange
        var positionId = await CreatePositionWithNoDepartments();
        var department = await CreateActiveDepartment("DEPT", "Отдел");
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([
                    department.Id.Value,
                    department.Id.Value // Дубликат
                ]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_OneDepartmentNotFound_Should_Failure()
    {
        // arrange
        var positionId = await CreatePositionWithNoDepartments();
        var existingDepartment = await CreateActiveDepartment("EXISTDEPT", "Существующий отдел");
        var nonExistentDepartmentId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([
                    existingDepartment.Id.Value,
                    nonExistentDepartmentId
                ]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_AllDepartmentsNotFound_Should_Failure()
    {
        // arrange
        var positionId = await CreatePositionWithNoDepartments();
        var nonExistentDepartmentId1 = Guid.NewGuid();
        var nonExistentDepartmentId2 = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([
                    nonExistentDepartmentId1,
                    nonExistentDepartmentId2
                ]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_DepartmentIsInactive_Should_Failure()
    {
        // arrange
        var positionId = await CreatePositionWithNoDepartments();
        var inactiveDepartment = await CreateInactiveDepartment("INACTIVE", "Неактивный отдел");
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([inactiveDepartment.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_When_PositionIsDeleted_Should_Failure()
    {
        // arrange
        var positionId = await CreateDeletedPosition();
        var department = await CreateActiveDepartment("DEPT", "Отдел");
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([department.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_Should_HandleLargeNumberOfDepartments()
    {
        // arrange
        var positionId = await CreatePositionWithNoDepartments();
        var departmentIds = new List<Guid>();
        
        for (int i = 0; i < 10; i++)
        {
            var department = await CreateActiveDepartment($"DEPT{i}", $"Отдел {i + 1}");
            departmentIds.Add(department.Id.Value);
        }
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest(departmentIds));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .Include(p => p.DepartmentPositions)
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(10, result.Value.Count);
            Assert.Equal(10, position.DepartmentPositions.Count);
        });
    }
    
    [Fact]
    public async Task UpdatePositionDepartments_Should_UpdateTimestamp()
    {
        // arrange
        (PositionId positionId, List<Guid> existingDepartmentIds) = await CreateTestData();
        var originalUpdatedAt = await GetPositionUpdatedAt(positionId);
        
        var newDepartment = await CreateActiveDepartment("NEWDEPT", "Новый отдел");
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionDepartmentsCommand(
                positionId.Value,
                new UpdatePositionDepartmentsRequest([newDepartment.Id.Value]));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.True(position.UpdatedAt > originalUpdatedAt);
        });
    }

    private async Task<(PositionId PositionId, List<Guid> DepartmentIds)> CreateTestData()
    {
        return await ExecuteInDb(async dbContext =>
        {
            // Создаем позицию
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("Разработчик").Value,
                PositionDescription.Create("Описание").Value,
                new List<DepartmentPosition>()).Value;
            
            // Создаем два активных отдела
            var department1 = await CreateDepartmentInDbContext(
                dbContext, "DEV", "Отдел разработки");
            
            var department2 = await CreateDepartmentInDbContext(
                dbContext, "QATEST", "Отдел тестирования");
            
            // Создаем связи
            var departmentPositions = new List<DepartmentPosition>
            {
                new DepartmentPosition(department1.Id, positionId),
                new DepartmentPosition(department2.Id, positionId),
            };
            
            position.UpdateDepartments(departmentPositions);
            
            // Сохраняем все
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return (positionId, new List<Guid> { department1.Id.Value, department2.Id.Value });
        });
    }
    
    private async Task<PositionId> CreatePositionWithNoDepartments()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("Позиция").Value,
                PositionDescription.Create("Описание").Value,
                new List<DepartmentPosition>()).Value;
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<Department> CreateActiveDepartment(string identifier, string name)
    {
        return await ExecuteInDb(async dbContext =>
        {
            return await CreateDepartmentInDbContext(dbContext, identifier, name);
        });
    }
    
    private async Task<Department> CreateInactiveDepartment(string identifier, string name)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var department = await CreateDepartmentInDbContext(dbContext, identifier, name);
            department.SoftDelete();
            await dbContext.SaveChangesAsync();
            return department;
        });
    }
    
    private async Task<Department> CreateDepartmentInDbContext(
        DirectoryServiceDbContext dbContext,
        string identifier,
        string name)
    {
        var departmentId = new DepartmentId(Guid.NewGuid());
        var departmentIdentifier = DepartmentIdentifier.Create(identifier).Value;
        var departmentPath = DepartmentPath.CreateParent(departmentIdentifier).Value;
        
        var department = Department.CreateParent(
            DepartmentName.Create(name).Value,
            departmentIdentifier,
            departmentPath,
            depth: 0,
            new List<DepartmentLocation>(),
            departmentId).Value;
        
        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync();
        
        return department;
    }
    
    private async Task<PositionId> CreateDeletedPosition()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("Удаленная позиция").Value,
                PositionDescription.Create("Описание").Value,
                new List<DepartmentPosition>()).Value;
            
            position.SoftDelete();
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<DateTime> GetPositionUpdatedAt(PositionId positionId)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId);
            
            return position.UpdatedAt;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdatePositionDepartmentsHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<UpdatePositionDepartmentsHandler>();
        
        return await action(sut);
    }
}