using AutoMapper;
using Moq;
using Xunit;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Application.TodoLists.Queries.GetTodos;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Entity = Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using NUnit.Framework;
using Todo_App.Application.Common.Mappings;

namespace Todo_App.Application.UnitTests.Features.TodoList
{
    public class GetTodosQueryTests
    {
        private Mock<IApplicationDbContext> _mockDbContext;
        private IMapper _mapper;
        private GetTodosQueryHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockDbContext = new Mock<IApplicationDbContext>();
            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            });

            _mapper = configuration.CreateMapper();
            _handler = new GetTodosQueryHandler(_mockDbContext.Object, _mapper);
        }

        [Test]
        public async Task Handle_ShouldReturnTodosVm_WhenDataExists()
        {
            // Arrange
            var todoLists = new List<Entity.TodoList>
            {
                new Entity.TodoList { Id = 1, Title = "List 1" },
                new Entity.TodoList { Id = 2, Title = "List 2" }
            }.AsQueryable();

            var tags = new List<Entity.Tags>
            {
                new Entity.Tags { Id = 1, Name = "Tag 1" },
                new Entity.Tags { Id = 2, Name = "Tag 2" }
            }.AsQueryable();

            var mockTodoListsDbSet = new Mock<DbSet<Entity.TodoList>>();
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.Provider).Returns(todoLists.Provider);
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.Expression).Returns(todoLists.Expression);
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.ElementType).Returns(todoLists.ElementType);
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.GetEnumerator()).Returns(todoLists.GetEnumerator());

            var mockTagsDbSet = new Mock<DbSet<Entity.Tags>>();
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.Provider).Returns(tags.Provider);
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.Expression).Returns(tags.Expression);
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.ElementType).Returns(tags.ElementType);
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.GetEnumerator()).Returns(tags.GetEnumerator());

            _mockDbContext.Setup(x => x.TodoLists).Returns(mockTodoListsDbSet.Object);
            _mockDbContext.Setup(x => x.Tags).Returns(mockTagsDbSet.Object);

            //_mapper.Map<Entity.TodoList, TodoListDto>(todoLists);
            //_mapper.Map<Entity.Tags, TagDto>(new Entity.Tags());

            var query = new GetTodosQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equals(2, result.Lists.Count);
            Assert.Equals(2, result.Tags.Count);
            Assert.Equals(Enum.GetValues(typeof(PriorityLevel)).Length, result.PriorityLevels.Count);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyTodosVm_WhenNoDataExists()
        {
            // Arrange
            var mockTodoListsDbSet = new Mock<DbSet<Entity.TodoList>>();
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.Provider).Returns(new List<Entity.TodoList>().AsQueryable().Provider);
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.Expression).Returns(new List<Entity.TodoList>().AsQueryable().Expression);
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.ElementType).Returns(new List<Entity.TodoList>().AsQueryable().ElementType);
            mockTodoListsDbSet.As<IQueryable<Entity.TodoList>>().Setup(m => m.GetEnumerator()).Returns(new List<Entity.TodoList>().AsQueryable().GetEnumerator());

            var mockTagsDbSet = new Mock<DbSet<Entity.Tags>>();
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.Provider).Returns(new List<Entity.Tags>().AsQueryable().Provider);
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.Expression).Returns(new List<Entity.Tags>().AsQueryable().Expression);
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.ElementType).Returns(new List<Entity.Tags>().AsQueryable().ElementType);
            mockTagsDbSet.As<IQueryable<Entity.Tags>>().Setup(m => m.GetEnumerator()).Returns(new List<Entity.Tags>().AsQueryable().GetEnumerator());

            _mockDbContext.Setup(x => x.TodoLists).Returns(mockTodoListsDbSet.Object);
            _mockDbContext.Setup(x => x.Tags).Returns(mockTagsDbSet.Object);

            _mapper.Map<Entity.TodoItem, TodoItemDto>(new Entity.TodoItem());
            _mapper.Map<Entity.TodoList, TodoListDto>(new Entity.TodoList());
            _mapper.Map<Entity.Tags, TagDto>(new Entity.Tags());

            var query = new GetTodosQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.IsEmpty(result.Lists);
            Assert.IsEmpty(result.Tags);
            Assert.Equals(Enum.GetValues(typeof(PriorityLevel)).Length, result.PriorityLevels.Count);
        }
    }
}
