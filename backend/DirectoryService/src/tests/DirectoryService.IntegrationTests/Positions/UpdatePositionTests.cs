using DirectoryService.Application.Positions.Command.Update;
using DirectoryService.Contracts.Positions;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Positions;

public class UpdatePositionTests : DirectoryBaseTests
{
    public UpdatePositionTests(DirectoryTestWebFactory factory)
        : base(factory)
    { }
    
    [Fact]
    public async Task UpdatePosition_Should_Succeed()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Старая должность").Value,
            PositionDescription.Create("Старое описание должности").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Новая должность",
                    "Новое описание должности"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Новая должность", position.Name.Value);
            Assert.Equal("Новое описание должности", position.Description!.Value);
            Assert.True(position.UpdatedAt > position.CreatedAt);
            Assert.True(position.IsActive);
            Assert.Null(position.DeletedAt);
        });
    }
    
    [Fact]
    public async Task UpdatePosition_Should_UpdateOnlyName()
    {
        // arrange
        var originalDescription = "Подробное описание должности";
        var positionId = await CreateActivePosition(
            PositionName.Create("Разработчик").Value,
            PositionDescription.Create(originalDescription).Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Старший разработчик",
                    originalDescription));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Старший разработчик", position.Name.Value);
            Assert.Equal(originalDescription, position.Description!.Value);
            Assert.True(position.UpdatedAt > position.CreatedAt);
        });
    }
    
    [Fact]
    public async Task UpdatePosition_Should_UpdateOnlyDescription()
    {
        // arrange
        var originalName = "Менеджер проектов";
        var positionId = await CreateActivePosition(
            PositionName.Create(originalName).Value,
            PositionDescription.Create("Старое описание").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    originalName,
                    "Новое расширенное описание с дополнительными обязанностями"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal(originalName, position.Name.Value);
            Assert.Equal("Новое расширенное описание с дополнительными обязанностями", position.Description!.Value);
            Assert.True(position.UpdatedAt > position.CreatedAt);
        });
    }
    
    [Fact]
    public async Task UpdatePosition_Should_UpdateWithEmptyDescription()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Аналитик").Value,
            PositionDescription.Create("Описание аналитика").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Ведущий аналитик",
                    string.Empty));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ведущий аналитик", position.Name.Value);
            Assert.Equal(string.Empty, position.Description!.Value);
        });
    }
    
    [Fact]
    public async Task UpdatePosition_When_PositionIdNonExistent_Should_Failure()
    {
        // arrange
        var nonExistentPositionId = Guid.NewGuid();
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                nonExistentPositionId,
                new UpdatePositionRequest(
                    "Новая должность",
                    "Описание"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_When_PositionIdIsEmpty_Should_Failure()
    {
        // arrange
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                Guid.Empty,
                new UpdatePositionRequest(
                    "Должность",
                    "Описание"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_When_NameIsEmpty_Should_Failure()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Тестировщик").Value,
            PositionDescription.Create("Описание").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    string.Empty,
                    "Новое описание"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_When_NameTooShort_Should_Failure()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Дир").Value,
            PositionDescription.Create("Описание").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Ди", // Меньше 3 символов
                    "Описание"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_When_NameTooLong_Should_Failure()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Менеджер").Value,
            PositionDescription.Create("Описание").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    new string('А', 101), // Больше 100 символов
                    "Описание"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_When_DescriptionTooLong_Should_Failure()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Разработчик").Value,
            PositionDescription.Create("Короткое описание").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Разработчик",
                    new string('О', 1001))); // Больше 1000 символов
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_When_DeletedPosition_Should_Failure()
    {
        // arrange
        var positionId = await CreateDeletedPosition(
            PositionName.Create("Удаленная должность").Value,
            PositionDescription.Create("Описание удаленной должности").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Восстановленная должность",
                    "Новое описание"));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        Assert.True(result.IsFailure);
        Assert.NotEmpty(result.Error);
    }
    
    [Fact]
    public async Task UpdatePosition_Should_TrimNameAndDescription()
    {
        // arrange
        var positionId = await CreateActivePosition(
            PositionName.Create("Разработчик").Value,
            PositionDescription.Create("Описание").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "   Ведущий разработчик   ",
                    "   Подробное описание с пробелами   "));
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Ведущий разработчик", position.Name.Value);
            Assert.Equal("Подробное описание с пробелами", position.Description!.Value);
        });
    }
    
    [Fact]
    public async Task UpdatePosition_Should_PreserveNullDescription()
    {
        // arrange
        var positionId = await CreateActivePositionWithNullDescription(
            PositionName.Create("Дизайнер").Value);
        
        var cancellationToken = CancellationToken.None;
        
        // act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdatePositionCommand(
                positionId.Value,
                new UpdatePositionRequest(
                    "Старший дизайнер",
                    string.Empty)); // Пустая строка становится пустым описанием
            
            return sut.Handle(command, cancellationToken);
        });
        
        // assert
        await ExecuteInDb(async dbContext =>
        {
            var position = await dbContext.Positions
                .FirstAsync(p => p.Id == positionId, cancellationToken);

            Assert.True(result.IsSuccess);
            Assert.Equal("Старший дизайнер", position.Name.Value);
            Assert.Equal(string.Empty, position.Description!.Value);
        });
    }

    private async Task<PositionId> CreateActivePosition(
        PositionName name,
        PositionDescription description)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            
            var position = Position.Create(
                positionId,
                name,
                description,
                new List<DepartmentPosition>()).Value;
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<PositionId> CreateActivePositionWithNullDescription(
        PositionName name)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            
            var position = Position.Create(
                positionId,
                name,
                null,
                new List<DepartmentPosition>()).Value;
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<PositionId> CreateDeletedPosition(
        PositionName name,
        PositionDescription description)
    {
        return await ExecuteInDb(async dbContext =>
        {
            var positionId = new PositionId(Guid.NewGuid());
            
            var position = Position.Create(
                positionId,
                name,
                description,
                new List<DepartmentPosition>()).Value;
            
            position.SoftDelete();
            
            dbContext.Positions.Add(position);
            await dbContext.SaveChangesAsync();

            return positionId;
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdatePositionHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        
        var sut = scope.ServiceProvider.GetRequiredService<UpdatePositionHandler>();
        
        return await action(sut);
    }
}