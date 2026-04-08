using CollabSpace.Models.DTOs.Board;
using FluentValidation;

namespace CollabSpace.Validators
{
    public class CreateBoardDtoValidator : AbstractValidator<CreateBoardDto>
    {
        public CreateBoardDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Board name is required.")
                .MinimumLength(2).WithMessage("Name must be at least 2 characters.")
                .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        }
    }
}