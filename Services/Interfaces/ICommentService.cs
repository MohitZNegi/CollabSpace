using CollabSpace.Models.DTOs.Comment;

namespace CollabSpace.Services.Interfaces
{
    public interface ICommentService
    {
        // Returns all comments for a card as a nested tree
        Task<List<CommentResponseDto>> GetCommentsAsync(
            Guid cardId, Guid requestingUserId);

        Task<CommentResponseDto> CreateCommentAsync(
            Guid cardId, CreateCommentDto request, Guid authorId);

        Task<CommentResponseDto> EditCommentAsync(
            Guid commentId, EditCommentDto request, Guid requestingUserId);

        Task DeleteCommentAsync(
            Guid commentId, Guid requestingUserId);
    }
}