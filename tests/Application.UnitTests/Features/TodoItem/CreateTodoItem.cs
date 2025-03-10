using Moq;
using Xunit;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoItems.Commands.CreateTodoItem;
using Entity = Todo_App.Domain.Entities;
using Todo_App.Domain.Events;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Todo_App.Application.UnitTests.Features.TodoItem;

public class CreateTodoItemCommandTests
{
    private Mock<IApplicationDbContext> _mockDbContext;
    private CreateTodoItemCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _handler = new CreateTodoItemCommandHandler(_mockDbContext.Object);
    }

    [Test]
    public async Task Handle_ShouldCreateTodoItem_WhenValidRequest()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = 1,
            Title = "New Todo",
            BackgroundColor = "#FF5733"
        };

        Entity.TodoItem addedItem = null;

        _mockDbContext.Setup(x => x.TodoItems.Add(It.IsAny<Entity.TodoItem>()))
        .Callback<Entity.TodoItem>(item =>
        {
            addedItem = item;
        });

        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => {
                if (addedItem != null)
                    addedItem.Id = 1;
            })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDbContext.Verify(x => x.TodoItems.Add(It.Is<Entity.TodoItem>(t => t.ListId == command.ListId && t.Title == command.Title && t.BackgroundColor == command.BackgroundColor)), Times.Once);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(1, result);
    }

    [Test]
    public async Task Handle_ShouldSetDefaultBackgroundColor_WhenBackgroundColorIsNull()
    {
        // Arrange
        var command = new CreateTodoItemCommand
        {
            ListId = 1,
            Title = "New Todo",
            BackgroundColor = null
        };

        Entity.TodoItem addedItem = null;

        _mockDbContext.Setup(x => x.TodoItems.Add(It.IsAny<Entity.TodoItem>()))
        .Callback<Entity.TodoItem>(item =>
        {
            addedItem = item;
        });

        _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => {
                if (addedItem != null)
                    addedItem.Id = 1;
            })
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockDbContext.Verify(x => x.TodoItems.Add(It.Is<Entity.TodoItem>(t => t.ListId == command.ListId && t.Title == command.Title && t.BackgroundColor == "#FFFFFF")), Times.Once);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(1, result);
    }
}
