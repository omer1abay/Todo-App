namespace Todo_App.Domain.Entities;
public class Tags : BaseAuditableEntity
{
    public string Name { get; set; } = null!;
    public virtual List<TodoItemTags> TodoItemTagsList { get; set; } = new();
}
