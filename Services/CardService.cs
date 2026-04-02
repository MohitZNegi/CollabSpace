using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Card;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class CardService : ICardService
    {
        private readonly AppDbContext _context;

        public CardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CardResponseDto>> GetCardsAsync(
            Guid boardId, Guid requestingUserId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId && !b.IsArchived);

            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            await RequireWorkspaceMemberAsync(board.WorkspaceId, requestingUserId);

            // Load cards ordered by Status then Position.
            // This groups Todo cards together in order,
            // then InProgress cards in order, then Done cards in order.
            // The frontend splits them into columns by Status.
            return await _context.Cards
                .Where(c => c.BoardId == boardId)
                .Include(c => c.AssignedTo)
                .Include(c => c.CreatedBy)
                .OrderBy(c => c.Status)
                .ThenBy(c => c.Position)
                .Select(c => MapToDto(c))
                .ToListAsync();
        }

        public async Task<CardResponseDto> CreateCardAsync(
            Guid boardId, CreateCardDto request, Guid createdByUserId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId && !b.IsArchived);

            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            await RequireWorkspaceMemberAsync(board.WorkspaceId, createdByUserId);

            // Calculate next position within the Todo column.
            // Max() returns null if no cards exist, so we use ?? -1
            // which means the first card gets position 0.
            var maxPosition = await _context.Cards
                .Where(c => c.BoardId == boardId && c.Status == "Todo")
                .MaxAsync(c => (int?)c.Position) ?? -1;

            var card = new Card
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                Status = "Todo",
                Position = maxPosition + 1,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Cards.Add(card);
            await _context.SaveChangesAsync();

            await _context.Entry(card).Reference(c => c.CreatedBy).LoadAsync();

            return MapToDto(card);
        }

        public async Task<CardResponseDto> UpdateCardAsync(
            Guid cardId, UpdateCardDto request, Guid requestingUserId)
        {
            var card = await _context.Cards
                .Include(c => c.Board)
                .Include(c => c.AssignedTo)
                .Include(c => c.CreatedBy)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            await RequireWorkspaceMemberAsync(
                card.Board!.WorkspaceId, requestingUserId);

            // Validate status value
            var validStatuses = new[] { "Todo", "InProgress", "Done" };
            if (!validStatuses.Contains(request.Status))
                throw new ArgumentException(
                    "Status must be Todo, InProgress, or Done.");

            // Validate the assignee is a member of the workspace
            if (request.AssignedToUserId.HasValue)
            {
                var assigneeIsMember = await _context.WorkspaceMembers
                    .AnyAsync(wm =>
                        wm.WorkspaceId == card.Board.WorkspaceId
                        && wm.UserId == request.AssignedToUserId.Value);

                if (!assigneeIsMember)
                    throw new ArgumentException(
                        "Assigned user is not a member of this workspace.");
            }

            card.Title = request.Title.Trim();
            card.Description = request.Description?.Trim();
            card.AssignedToUserId = request.AssignedToUserId;
            card.Status = request.Status;
            card.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Reload the assignee navigation property after update
            if (card.AssignedToUserId.HasValue)
                await _context.Entry(card)
                    .Reference(c => c.AssignedTo).LoadAsync();

            return MapToDto(card);
        }

        public async Task MoveCardAsync(
            Guid cardId, MoveCardDto request, Guid requestingUserId)
        {
            var card = await _context.Cards
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            await RequireWorkspaceMemberAsync(
                card.Board!.WorkspaceId, requestingUserId);

            var validStatuses = new[] { "Todo", "InProgress", "Done" };
            if (!validStatuses.Contains(request.Status))
                throw new ArgumentException(
                    "Status must be Todo, InProgress, or Done.");

            var oldStatus = card.Status;
            var oldPosition = card.Position;
            var newStatus = request.Status;
            var newPosition = request.Position;

            // -------------------------------------------------------
            // REORDER LOGIC
            // -------------------------------------------------------
            // Moving within the same column:
            //   Shift cards between old and new position to fill the gap.
            //
            // Moving to a different column:
            //   1. Close the gap in the old column by shifting cards down.
            //   2. Open a gap in the new column by shifting cards up.
            //   3. Place the card in the new position.

            if (oldStatus == newStatus)
            {
                // Same column move
                if (oldPosition < newPosition)
                {
                    // Moving DOWN: shift cards between old+1 and new UP by one
                    var cardsToShift = await _context.Cards
                        .Where(c => c.BoardId == card.BoardId
                                 && c.Status == newStatus
                                 && c.Position > oldPosition
                                 && c.Position <= newPosition
                                 && c.Id != cardId)
                        .ToListAsync();

                    foreach (var c in cardsToShift) c.Position--;
                }
                else if (oldPosition > newPosition)
                {
                    // Moving UP: shift cards between new and old-1 DOWN by one
                    var cardsToShift = await _context.Cards
                        .Where(c => c.BoardId == card.BoardId
                                 && c.Status == newStatus
                                 && c.Position >= newPosition
                                 && c.Position < oldPosition
                                 && c.Id != cardId)
                        .ToListAsync();

                    foreach (var c in cardsToShift) c.Position++;
                }
            }
            else
            {
                // Cross-column move

                // Close the gap in the old column
                var oldColumnCards = await _context.Cards
                    .Where(c => c.BoardId == card.BoardId
                             && c.Status == oldStatus
                             && c.Position > oldPosition
                             && c.Id != cardId)
                    .ToListAsync();

                foreach (var c in oldColumnCards) c.Position--;

                // Open a gap in the new column
                var newColumnCards = await _context.Cards
                    .Where(c => c.BoardId == card.BoardId
                             && c.Status == newStatus
                             && c.Position >= newPosition
                             && c.Id != cardId)
                    .ToListAsync();

                foreach (var c in newColumnCards) c.Position++;
            }

            // Apply the move to the card itself
            card.Status = newStatus;
            card.Position = newPosition;
            card.UpdatedAt = DateTime.UtcNow;

            // Single SaveChangesAsync commits all position changes
            // atomically. Positions are never inconsistent.
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCardAsync(
            Guid cardId, Guid requestingUserId)
        {
            var card = await _context.Cards
                .Include(c => c.Board)
                .FirstOrDefaultAsync(c => c.Id == cardId);

            if (card == null)
                throw new KeyNotFoundException("Card not found.");

            var membership = await _context.WorkspaceMembers
                .FirstOrDefaultAsync(wm =>
                    wm.WorkspaceId == card.Board!.WorkspaceId
                    && wm.UserId == requestingUserId);

            if (membership == null)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            // Only Leads and Owners can delete cards
            if (membership.WorkspaceRole == "Member")
                throw new ForbiddenException(
                    "Only Leads and Owners can delete cards.");

            // Close the position gap left by the deleted card
            var cardsToShift = await _context.Cards
                .Where(c => c.BoardId == card.BoardId
                         && c.Status == card.Status
                         && c.Position > card.Position)
                .ToListAsync();

            foreach (var c in cardsToShift) c.Position--;

            _context.Cards.Remove(card);
            await _context.SaveChangesAsync();
        }

        // ---------------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------------

        // Reusable membership check used across multiple methods.
        // Throws ForbiddenException if the user is not a member.
        private async Task RequireWorkspaceMemberAsync(
            Guid workspaceId, Guid userId)
        {
            var isMember = await _context.WorkspaceMembers
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == userId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");
        }

        private static CardResponseDto MapToDto(Card card) => new()
        {
            Id = card.Id,
            BoardId = card.BoardId,
            Title = card.Title,
            Description = card.Description,
            AssignedToUserId = card.AssignedToUserId,
            AssignedToUsername = card.AssignedTo?.Username,
            Status = card.Status,
            Position = card.Position,
            CreatedByUsername = card.CreatedBy?.Username ?? "Unknown",
            CreatedAt = card.CreatedAt,
            UpdatedAt = card.UpdatedAt,
            CommentCount = card.Comments?.Count ?? 0
        };
    }
}