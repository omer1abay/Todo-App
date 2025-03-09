using AutoMapper;
using Todo_App.Application.Common.Mappings;
using Entity = Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;

public class TodoItemDto : IMapFrom<Entity.TodoItem>
{

    public TodoItemDto()
    {
        TodoItemTagsList = new List<Entity.TodoItemTags>();
    }

    public int Id { get; set; }

    public int ListId { get; set; }

    public string? Title { get; set; }

    public bool Done { get; set; }

    public int Priority { get; set; }

    public string? Note { get; set; }

    public string? BackgroundColor { get; set; }

    public IList<Entity.TodoItemTags> TodoItemTagsList { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Entity.TodoItem, TodoItemDto>()
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority));
    }
}
