using CollabSpace.Models.DTOs.Card;
using FluentValidation;

namespace CollabSpace.Validators
{
    public class CreateCardDtoValidator : AbstractValidator<CreateCardDto>
    {
        public CreateCardDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Card title is required.")
                .MinimumLength(1).WithMessage("Title cannot be empty.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");
        }
    }
}