using Moq;
using Xunit;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoLists.Commands.DeleteTodoList;
using Entity = Todo_App.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using NUnit.Framework;
using System.Linq.Expressions;

namespace Todo_App.Application.UnitTests.Features.TodoList
{
    public class DeleteTodoListCommandTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private DeleteTodoListCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            _handler = new DeleteTodoListCommandHandler(_mockDbContext.Object);
        }

        [Test]
        public async Task Handle_ShouldDeleteTodoList_WhenTodoListExists()
        {
            // Arrange
            var todoList = new Entity.TodoList
            {
                Id = 1,
                IsActive = true,
                Items = new List<Entity.TodoItem> { new Entity.TodoItem { Id = 1, IsActive = true } }
            };

            // Doğru mock kurulumu - queryable mock
            var mockQueryable = new List<Entity.TodoList> { todoList }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Entity.TodoList>>();
            mockDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.Provider).Returns(mockQueryable.Provider);
            mockDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.Expression).Returns(mockQueryable.Expression);
            mockDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.ElementType).Returns(mockQueryable.ElementType);
            mockDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.GetEnumerator()).Returns(mockQueryable.GetEnumerator());

            // Include metodu için kurulum
            mockDbSet.Setup(x => x.Include(It.IsAny<string>())).Returns(mockDbSet.Object);

            _mockDbContext.Setup(x => x.TodoLists).Returns(mockDbSet.Object);
            _mockDbContext.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var command = new DeleteTodoListCommand(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(todoList.IsActive);

            // Items koleksiyonunu doğrulama - All yerine foreach kullanma
            foreach (var item in todoList.Items)
            {
                Assert.False(item.IsActive);
            }

            _mockDbContext.Verify(x => x.TodoLists.Update(It.Is<Entity.TodoList>(t => t.Id == 1 && !t.IsActive)), Times.Once);
            _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldThrowNotFoundException_WhenTodoListDoesNotExist()
        {
            // Arrange
            _mockDbContext.Setup(x => x.TodoLists.Include(It.IsAny<Expression<Func<Entity.TodoList, object>>>()).Where(It.IsAny<Expression<Func<Entity.TodoList, bool>>>()))
                .Returns(new List<Entity.TodoList>().AsQueryable());

            var command = new DeleteTodoListCommand(1);

            // Act & Assert
            Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
        }
    }
}
