using Microsoft.EntityFrameworkCore;
using Entity = Todo_App.Domain.Entities;

namespace Todo_App.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Entity.TodoList> TodoLists { get; }

    DbSet<Entity.TodoItem> TodoItems { get; }

    DbSet<Entity.Tags> Tags { get; }

    DbSet<Entity.TodoItemTags> TodoItemTags { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
