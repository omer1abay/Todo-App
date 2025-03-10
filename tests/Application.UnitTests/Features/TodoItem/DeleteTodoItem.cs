using Moq;
using Xunit;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoItems.Commands.DeleteTodoItem;
using Entity = Todo_App.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Todo_App.Application.UnitTests.Features.TodoItem
{
    public class DeleteTodoItemCommandTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private DeleteTodoItemCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _handler = new DeleteTodoItemCommandHandler(_mockDbContext.Object);
        }

        [Test]
        public async Task Handle_ShouldDeleteTodoItem_WhenTodoItemExists()
        {
            // Arrange
            var todoItem = new Entity.TodoItem { Id = 1, IsActive = true };
            _mockDbContext.Setup(x => x.TodoItems.FindAsync(new object[] { 1 }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(todoItem);
            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new DeleteTodoItemCommand(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(todoItem.IsActive);
            _mockDbContext.Verify(x => x.TodoItems.Update(It.Is<Entity.TodoItem>(t => t.Id == 1 && !t.IsActive)), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldThrowNotFoundException_WhenTodoItemDoesNotExist()
        {
            // Arrange
            _mockDbContext.Setup(x => x.TodoItems.FindAsync(new object[] { 1 }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Entity.TodoItem)null);

            var command = new DeleteTodoItemCommand(1);

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
