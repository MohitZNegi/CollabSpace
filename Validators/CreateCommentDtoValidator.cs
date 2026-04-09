using CollabSpace.Models.DTOs.Comment;
using FluentValidation;

namespace CollabSpace.Validators
{
    public class CreateCommentDtoValidator
        : AbstractValidator<CreateCommentDto>
    {
        public CreateCommentDtoValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty().WithMessage("Comment cannot be empty.")
                .MaximumLength(2000)
                    .WithMessage("Comment cannot exceed 2000 characters.");
        }
    }
}