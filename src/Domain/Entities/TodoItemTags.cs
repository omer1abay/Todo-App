using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Todo_App.Domain.Entities;
public class TodoItemTags: BaseEntity
{
    public int TodoItemId { get; set; }
    public virtual TodoItem? TodoItem { get; set; }
    public int TagId { get; set; }
    public virtual Tags? Tag { get; set; }
}
