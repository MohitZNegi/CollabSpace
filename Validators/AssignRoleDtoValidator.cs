using CollabSpace.Models.DTOs.WorkSpace;
using FluentValidation;

namespace CollabSpace.Validators
{
    public class AssignRoleDtoValidator : AbstractValidator<AssignRoleDto>
    {
        public AssignRoleDtoValidator()
        {
            RuleFor(x => x.Role)
                .NotEmpty()
                    .WithMessage("Role is required.")
                .Must(r => r == "Lead" || r == "Member")
                    .WithMessage("Role must be 'Lead' or 'Member'.");
        }
    }
}