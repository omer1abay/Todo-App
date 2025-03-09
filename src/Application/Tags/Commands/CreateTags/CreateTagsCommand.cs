using MediatR;
using Todo_App.Application.Common.Interfaces;

namespace Todo_App.Application.Tags.Commands.CreateTags;
public record CreateTagsCommand(string name) : IRequest<int>;

public class CreateTagsCommandHandler : IRequestHandler<CreateTagsCommand, int>
{
    private readonly IApplicationDbContext _context;
    public CreateTagsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<int> Handle(CreateTagsCommand request, CancellationToken cancellationToken)
    {
        var insertedData = new Domain.Entities.Tags() { Name = request.name };
        await _context.Tags.AddAsync(insertedData);
        await _context.SaveChangesAsync(cancellationToken);
        return insertedData.Id;
    }
}
