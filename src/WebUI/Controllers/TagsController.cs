using Microsoft.AspNetCore.Mvc;
using Todo_App.Application.Tags.Commands.CreateTags;

namespace Todo_App.WebUI.Controllers;
public class TagsController : ApiControllerBase
{

    [HttpPost("[action]")]
    public async Task<ActionResult<int>> CreateTag(CreateTagsCommand command)
    {
        var id = await Mediator.Send(command);
        return id;
    }
}
