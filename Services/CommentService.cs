using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.Constants;
using CollabSpace.Models.DTOs.Comment;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notifications;
        private readonly IActivityService _activity;

        public CommentService(
            AppDbContext context,
            INotificationService notifications,
            IActivityService activity)
        {
            _context = context;
            _notifications = notifications;
            _activity = activity;
        }

        public async Task<List<CommentResponseDto>> GetCommentsAsync(
            Guid cardId, Guid requestingUserId)
        {
            var card = await _context.Cards
                .AsNoTracking()
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            await RequireWorkspaceMemberAsync(
                card.Board!.WorkspaceId, requestingUserId);

            // Load ALL comments for this card in one query,
            // including their authors. We build the tree in memory
            // rather than with multiple queries or recursive SQL.
            var allComments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.CardId == cardId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            return BuildCommentTree(allComments, null);
        }

        public async Task<CommentResponseDto> CreateCommentAsync(
            Guid cardId, CreateCommentDto request, Guid authorId)
        {
            var card = await _context.Cards
                .AsNoTracking()
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            await RequireWorkspaceMemberAsync(
                card.Board!.WorkspaceId, authorId);

            // If this is a reply, verify the parent comment exists
            // and belongs to the same card
            if (request.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments
                    .AsNoTracking()
                    .AnyAsync(c => c.Id == request.ParentCommentId
                                && c.CardId == cardId);

                if (!parentExists)
                    throw new KeyNotFoundException(
                        "Parent comment not found on this card.");
            }

            var author = await _context.Users
                .AsNoTracking()
                .FirstAsync(u => u.Id == authorId);

            var comment = new Comment
            {
                Id = Guid.NewGuid(),
                CardId = cardId,
                UserId = authorId,
                Content = request.Content.Trim(),
                ParentCommentId = request.ParentCommentId,
                CreatedAt = DateTime.UtcNow,
                IsEdited = false
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            await _activity.RecordAsync(
                card.Board!.WorkspaceId, authorId,
                ActivityTypes.CommentAdded,
                $"{author.Username} commented on \"{card.Title}\"",
                comment.Id, "Comment");

            // -------------------------------------------------------
            // NOTIFICATION TRIGGERS
            // -------------------------------------------------------

            // Notify the card owner if someone else comments
            if (card.CreatedByUserId != authorId)
            {
                await _notifications.NotifyCommentAddedAsync(
                    card.CreatedByUserId,
                    card.Title,
                    author.Username,
                    comment.Id,
                    authorId);
            }

            // Parse @mentions from the comment content.
            // Find all @username patterns and notify those users.
            var mentionedUserIds = await ResolveMentionsAsync(
                request.Content, card.Board!.WorkspaceId, authorId);

            if (mentionedUserIds.Any())
            {
                await _notifications.NotifyMentionsAsync(
                    mentionedUserIds,
                    author.Username,
                    card.Title,
                    comment.Id);
            }

            return new CommentResponseDto
            {
                Id = comment.Id,
                CardId = comment.CardId,
                UserId = comment.UserId,
                Username = author.Username,
                AvatarUrl = author.AvatarUrl,
                Content = comment.Content,
                ParentCommentId = comment.ParentCommentId,
                IsEdited = comment.IsEdited,
                CreatedAt = comment.CreatedAt,
            };
        }

        public async Task<CommentResponseDto> EditCommentAsync(
            Guid commentId, EditCommentDto request, Guid requestingUserId)
        {
            var comment = await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            // Only the author can edit their own comment.
            // Admins can delete but not edit others' comments.
            if (comment.UserId != requestingUserId)
                throw new ForbiddenException(
                    "You can only edit your own comments.");

            comment.Content = request.Content.Trim();
            comment.IsEdited = true;

            await _context.SaveChangesAsync();
            return MapToDto(comment);
        }

        public async Task DeleteCommentAsync(
            Guid commentId, Guid requestingUserId)
        {
            var comment = await _context.Comments
                .Include(c => c.Replies)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            if (comment == null)
                throw new KeyNotFoundException("Comment not found.");

            // Check if the requester is the author or a global Admin
            var requester = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == requestingUserId);

            var isAuthor = comment.UserId == requestingUserId;
            var isAdmin = requester?.GlobalRole == "Admin";

            if (!isAuthor && !isAdmin)
                throw new ForbiddenException(
                    "You can only delete your own comments.");

            // Delete all replies first to avoid FK constraint violations.
            // Restrict delete behaviour on the self-reference means the
            // database will not cascade automatically.
            if (comment.Replies.Any())
                _context.Comments.RemoveRange(comment.Replies);

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        // ---------------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------------

        // Builds a nested tree from a flat list of comments.
        // Called recursively: first call passes null for parentId
        // to get all top-level comments, then each comment's
        // Replies are built by calling this with that comment's ID.
        //
        // This is more efficient than making one query per level
        // because all comments are loaded once and the tree is
        // built entirely in memory.
        private List<CommentResponseDto> BuildCommentTree(
            List<Comment> allComments, Guid? parentId)
        {
            return allComments
                .Where(c => c.ParentCommentId == parentId)
                .Select(c =>
                {
                    var dto = MapToDto(c);
                    dto.Replies = BuildCommentTree(allComments, c.Id);
                    return dto;
                })
                .ToList();
        }

        // Parses @username mentions from comment text.
        // Finds workspace members whose usernames match and
        // returns their IDs for notification, excluding the author.
        private async Task<List<Guid>> ResolveMentionsAsync(
            string content, Guid workspaceId, Guid authorId)
        {
            // Extract all @word patterns from the content
            var words = content.Split(' ', '\n', '\r')
                .Where(w => w.StartsWith('@') && w.Length > 1)
                .Select(w => w.TrimStart('@').Trim())
                .Distinct()
                .ToList();

            if (!words.Any()) return new List<Guid>();

            // Find workspace members whose usernames match the mentions
            return await _context.WorkspaceMembers
                .AsNoTracking()
                .Where(wm => wm.WorkspaceId == workspaceId
                          && wm.UserId != authorId)
                .Include(wm => wm.User)
                .Where(wm => words.Contains(wm.User!.Username))
                .Select(wm => wm.UserId)
                .ToListAsync();
        }

        private async Task RequireWorkspaceMemberAsync(
            Guid workspaceId, Guid userId)
        {
            var isMember = await _context.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == userId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");
        }

        private static CommentResponseDto MapToDto(Comment c) => new()
        {
            Id = c.Id,
            CardId = c.CardId,
            UserId = c.UserId,
            Username = c.User?.Username ?? "Unknown",
            AvatarUrl = c.User?.AvatarUrl,
            Content = c.Content,
            ParentCommentId = c.ParentCommentId,
            IsEdited = c.IsEdited,
            CreatedAt = c.CreatedAt,
        };
    }
}
