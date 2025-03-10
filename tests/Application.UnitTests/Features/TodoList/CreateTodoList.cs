using Moq;
using Xunit;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoLists.Commands.CreateTodoList;
using Entity = Todo_App.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Todo_App.Application.UnitTests.Features.TodoList
{
    public class CreateTodoListCommandTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private CreateTodoListCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _handler = new CreateTodoListCommandHandler(_mockDbContext.Object);
        }

        [Test]
        public async Task Handle_ShouldCreateTodoList_WhenValidRequest()
        {
            // Arrange
            var command = new CreateTodoListCommand
            {
                Title = "New Todo List"
            };

            Entity.TodoList addedItem = null;

            _mockDbContext.Setup(x => x.TodoLists.Add(It.IsAny<Entity.TodoList>()))
        .Callback<Entity.TodoList>(item =>
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
            _mockDbContext.Verify(x => x.TodoLists.Add(It.Is<Entity.TodoList>(t => t.Title == command.Title)), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.AreEqual(1, result);
        }

        [Test]
        public async Task Handle_ShouldThrowException_WhenTitleIsNull()
        {
            // Arrange
            var command = new CreateTodoListCommand
            {
                Title = null
            };

            Entity.TodoList addedItem = null;

            _mockDbContext.Setup(x => x.TodoLists.Add(It.IsAny<Entity.TodoList>()))
         .Callback<Entity.TodoList>(item =>
         {
             addedItem = item;
         });
            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
             .Callback(() => {
                 if (addedItem != null)
                     addedItem.Id = 1;
             })
             .ReturnsAsync(1);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => Task.Run(() => _handler.Handle(command, CancellationToken.None)));
        }
    }
}
