using MediatR;
using Microsoft.EntityFrameworkCore;
using Todo_App.Application.Common.Exceptions;
using Todo_App.Application.Common.Interfaces;
using Todo_App.Domain.Entities;
using Todo_App.Domain.Enums;

namespace Todo_App.Application.TodoItems.Commands.UpdateTodoItemDetail;

public record UpdateTodoItemDetailCommand : IRequest
{
    public int Id { get; init; }

    public int ListId { get; init; }

    public PriorityLevel Priority { get; init; }

    public string? Note { get; init; }

    public List<int> Tags { get; init; } = new();
}

public class UpdateTodoItemDetailCommandHandler : IRequestHandler<UpdateTodoItemDetailCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateTodoItemDetailCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(UpdateTodoItemDetailCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .FirstOrDefaultAsync(x => x.Id == request.Id , cancellationToken);

        var todoItemTags = await _context.TodoItemTags
            .Where(x => x.TodoItemId == request.Id)
            .ToListAsync(cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        if (request.Tags.Count > 0 || todoItemTags.Any())
        {
            entity.TodoItemTagsList.Except(todoItemTags.Where(x => request.Tags.Contains(x.TagId))).ToList().ForEach(tag => entity.TodoItemTagsList.Remove(tag));
            entity.TodoItemTagsList.AddRange(request.Tags.Select(tagId => new TodoItemTags { TagId = tagId, TodoItemId = request.Id }).ToList());
            entity.TodoItemTagsList = entity.TodoItemTagsList.DistinctBy(x => x.TagId).ToList();
        }

        entity.ListId = request.ListId;
        entity.Priority = request.Priority;
        entity.Note = request.Note;

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
