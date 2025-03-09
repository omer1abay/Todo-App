using FluentValidation;
using Todo_App.Application.Common.Interfaces;

namespace Todo_App.Application.Tags.Commands.CreateTags;
internal class CreateTagsCommandValidator : AbstractValidator<CreateTagsCommand>
{
    //private readonly IApplicationDbContext _context;

    //public CreateTagsCommandValidator(IApplicationDbContext context)
    //{
    //    _context = context;

    //    RuleFor(v => v.name)
    //        .MustAsync(BeUniqueName).WithMessage("The specified name already exists.");
    //}
}
