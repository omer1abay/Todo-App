using Moq;
using NUnit.Framework;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;
using Entity = Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;
//using Xunit;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace Todo_App.Application.UnitTests.Features.TodoItem;

public class UpdateItemDetailTests
{
    private Mock<IApplicationDbContext> _mockDbContext;
    private UpdateTodoItemDetailCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _mockDbContext = new Mock<IApplicationDbContext>();
        _handler = new UpdateTodoItemDetailCommandHandler(_mockDbContext.Object);
    }

    [Test]
    public async Task Handle_ShouldUpdateTodoItem_WhenTodoItemExists()
    {
        // Arrange
        var todoItem = new Entity.TodoItem { Id = 1, ListId = 1, Priority = PriorityLevel.None, Note = "Old Note" };
        _mockDbContext.Setup(x => x.TodoItems.FirstOrDefaultAsync(It.IsAny<Expression<Func<Entity.TodoItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(todoItem);
        _mockDbContext.Setup(x => x.TodoItemTags.Where(It.IsAny<Expression<Func<Entity.TodoItemTags, bool>>>()))
            .Returns(new List<Entity.TodoItemTags>().AsQueryable());

        var command = new UpdateTodoItemDetailCommand
        {
            Id = 1,
            ListId = 2,
            Priority = PriorityLevel.High,
            Note = "New Note",
            Tags = new List<int> { 1, 2 },
            Reminder = DateTime.Now
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equals(2, todoItem.ListId);
        Assert.Equals(PriorityLevel.High, todoItem.Priority);
        Assert.Equals("New Note", todoItem.Note);
        Assert.Equals(command.Reminder, todoItem.Reminder);
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task Handle_ShouldThrowNotFoundException_WhenTodoItemDoesNotExist()
    {
        // Arrange
        var mockSet = new Mock<DbSet<Entity.TodoItem>>();
        var data = new List<Entity.TodoItem>().AsQueryable();

        // IQueryable kurulumu
        mockSet.As<IQueryable<Entity.TodoItem>>().Setup(m => m.Provider).Returns(data.Provider);
        mockSet.As<IQueryable<Entity.TodoItem>>().Setup(m => m.Expression).Returns(data.Expression);
        mockSet.As<IQueryable<Entity.TodoItem>>().Setup(m => m.ElementType).Returns(data.ElementType);
        mockSet.As<IQueryable<Entity.TodoItem>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

        // Async extension yapılandırması
        mockSet.As<IAsyncEnumerable<Entity.TodoItem>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Entity.TodoItem>(data.GetEnumerator()));

        mockSet.As<IQueryable<Entity.TodoItem>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Entity.TodoItem>(data.Provider));

        _mockDbContext.Setup(c => c.TodoItems).Returns(mockSet.Object);

        var command = new UpdateTodoItemDetailCommand { Id = 0 };

        Assert.Throws<NotFoundException>(() =>
                Task.Run(() => _handler.Handle(command, CancellationToken.None)));
    }

    [Test]
    public async Task Handle_ShouldUpdateTags_WhenTagsAreProvided()
    {
        // Arrange
        var todoItem = new Entity.TodoItem { Id = 1, ListId = 1, Priority = PriorityLevel.None, Note = "Old Note", TodoItemTagsList = new List<Entity.TodoItemTags>() };
        var existingTags = new List<Entity.TodoItemTags> { new Entity.TodoItemTags { TagId = 1, TodoItemId = 1 } };
        _mockDbContext.Setup(x => x.TodoItems.FindAsync(It.IsAny<int>()))
            .ReturnsAsync(todoItem);
        _mockDbContext.Setup(x => x.TodoItemTags.Where(It.IsAny<Expression<Func<Entity.TodoItemTags, bool>>>()))
    .Returns((Expression<Func<Entity.TodoItemTags, bool>> predicate) =>
        existingTags.Where(predicate.Compile()).AsQueryable());

        var command = new UpdateTodoItemDetailCommand
        {
            Id = 1,
            Tags = new List<int> { 2, 3 }
        };

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(todoItem.TodoItemTagsList.Any(t => t.TagId == 2));
        Assert.False(todoItem.TodoItemTagsList.Any(t => t.TagId == 3));
        Assert.False(todoItem.TodoItemTagsList.Any(t => t.TagId == 1));
        _mockDbContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

public class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }

}

public class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })
            .MakeGenericMethod(resultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { executionResult });
    }

    public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
    {
        return new TestAsyncEnumerable<TResult>(expression);
    }
}

public class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this.AsQueryable().Provider);
}