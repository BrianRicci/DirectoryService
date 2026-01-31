using DirectoryService.Application.Positions.Command.SoftDelete;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Positions;

public class SoftDeletePositionTests : DirectoryBaseTests
{
    public SoftDeletePositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task SoftDeletePosition_Should_Succeed()
    {
        // arrange
        var positionId = await CreateActivePosition();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(positionId.Value);
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(positionId.Value, result.Value);
            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);
            Assert.True(position.UpdatedAt > position.CreatedAt);
            Assert.Equal(position.DeletedAt.Value.Date, DateTime.UtcNow.Date);
        });
    }
    
    [Fact]
    public async Task SoftDeletePosition_When_PositionIdNonExistent_Should_Failure()
    {
        // arrange
        var nonExistentPositionId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(nonExistentPositionId);
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task SoftDeletePosition_When_PositionIdIsEmpty_Should_Failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(Guid.Empty);
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task SoftDeletePosition_With_DepartmentPositions_Should_Succeed()
    {
        // arrange
        var (positionId, departmentIds) = await CreatePositionWithDepartments();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(positionId.Value);
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.False(position.IsActive);
            Assert.NotNull(position.DeletedAt);
            
            // Проверяем что связи с департаментами остались
            var departmentPositions = await dbContext.DepartmentPositions
                .Where(dp => dp.PositionId == positionId)
                .ToListAsync(cancellationToken);
            
            Assert.Equal(2, departmentPositions.Count);
        });
    }
    
    [Fact]
    public async Task SoftDeletePosition_Should_WorkInTransaction()
    {
        // arrange
        var positionId = await CreateActivePosition();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(positionId.Value);
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.False(position.IsActive);
        });
    }
    
    [Fact]
    public async Task SoftDeletePosition_Should_UpdateTimestamp()
    {
        // arrange
        var positionId = await CreateActivePosition();
        var originalUpdatedAt = await GetPositionUpdatedAt(positionId);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(positionId.Value);
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
    
    [Fact]
    public async Task SoftDeletePosition_Should_CascadeToUpdatedAtOnly()
    {
        // arrange
        var positionId = await CreateActivePosition();
        await UpdatePosition(positionId); // Изменяем позицию перед удалением
        
        var originalUpdatedAt = await GetPositionUpdatedAt(positionId);
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new SoftDeletePositionCommand(positionId.Value);
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.True(position.UpdatedAt > originalUpdatedAt);
            Assert.NotNull(position.DeletedAt);
            Assert.Equal(position.UpdatedAt, position.DeletedAt.Value);
        });
    }

    private async Task<PositionId> CreateActivePosition()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("Активная позиция").Value,
                PositionDescription.Create("Описание активной позиции").Value,
                new List<DepartmentPosition>()).Value;
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<PositionId> CreateDeletedPosition()
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("Удаленная позиция").Value,
                PositionDescription.Create("Описание удаленной позиции").Value,
                new List<DepartmentPosition>()).Value;
            
            position.SoftDelete();
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<(PositionId PositionId, List<Guid> DepartmentIds)> CreatePositionWithDepartments()
    {
        return await ExecuteInDb(async dbContext =>
        {
            // Создаем позицию
            var positionId = new PositionId(Guid.NewGuid());
            var position = Position.Create(
                positionId,
                PositionName.Create("Позиция с отделами").Value,
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
    
    private async Task UpdatePosition(PositionId positionId)
    {
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId);
            
            position.Rename(PositionName.Create("Обновленное название").Value);
            await dbContext.SaveChangesAsync();
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
    
    private async Task<DateTime> GetPositionUpdatedAt(PositionId positionId)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId);
            
            return position.UpdatedAt;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<SoftDeletePositionHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<SoftDeletePositionHandler>();
        
        return await action(sut);
    }
}