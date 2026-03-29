using CollabSpace.Models.DTOs.WorkSpace;
using FluentValidation;

namespace CollabSpace.Validators
{
    public class CreateWorkspaceDtoValidator
        : AbstractValidator<CreateWorkspaceDto>
    {
        public CreateWorkspaceDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("Workspace name is required.")
                .MinimumLength(2)
                    .WithMessage("Name must be at least 2 characters.")
                .MaximumLength(100)
                    .WithMessage("Name cannot exceed 100 characters.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                    .WithMessage("Description cannot exceed 500 characters.")
                .When(x => x.Description != null);
        }
    }
}