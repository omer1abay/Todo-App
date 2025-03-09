using Todo_App.Application.Common.Mappings;
using Entity = Todo_App.Domain.Entities;

namespace Todo_App.Application.TodoLists.Queries.GetTodos;
public class TagDto : IMapFrom<Entity.Tags>
{
    public int Id { get; set; }
    public string? Name { get; set; }
}