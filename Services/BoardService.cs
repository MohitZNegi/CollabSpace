using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Board;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class BoardService : IBoardService
    {
        private readonly AppDbContext _context;

        public BoardService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<BoardResponseDto>> GetBoardsAsync(
            Guid workspaceId, Guid requestingUserId)
        {
            // Verify the requesting user is a member of the workspace
            var isMember = await _context.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == requestingUserId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            return await _context.Boards
                .AsNoTracking()
                .Where(b => b.WorkspaceId == workspaceId && !b.IsArchived)
                .Include(b => b.CreatedBy)
                .OrderBy(b => b.CreatedAt)
                .Select(b => MapToDto(b))
                .ToListAsync();
        }

        public async Task<BoardResponseDto> CreateBoardAsync(
            Guid workspaceId, CreateBoardDto request, Guid createdByUserId)
        {
            var isMember = await _context.WorkspaceMembers
                .AsNoTracking()
                .AnyAsync(wm => wm.WorkspaceId == workspaceId
                             && wm.UserId == createdByUserId);

            if (!isMember)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            var board = new Board
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                Name = request.Name.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                IsArchived = false
            };

            _context.Boards.Add(board);
            await _context.SaveChangesAsync();

            // Reload with the creator's username for the response
            await _context.Entry(board)
                .Reference(b => b.CreatedBy)
                .LoadAsync();

            return MapToDto(board);
        }

        public async Task ArchiveBoardAsync(
            Guid boardId, Guid requestingUserId)
        {
            var board = await _context.Boards
                .FirstOrDefaultAsync(b => b.Id == boardId && !b.IsArchived);

            if (board == null)
                throw new KeyNotFoundException("Board not found.");

            // Only Leads and Owners can archive boards — per your user stories
            var membership = await _context.WorkspaceMembers
                .AsNoTracking()
                .FirstOrDefaultAsync(wm => wm.WorkspaceId == board.WorkspaceId
                                        && wm.UserId == requestingUserId);

            if (membership == null)
                throw new ForbiddenException(
                    "You are not a member of this workspace.");

            if (membership.WorkspaceRole == "Member")
                throw new ForbiddenException(
                    "Only Leads and Owners can archive boards.");

            board.IsArchived = true;
            await _context.SaveChangesAsync();
        }

        // Static mapper keeps mapping logic in one place
        private static BoardResponseDto MapToDto(Board board) => new()
        {
            Id = board.Id,
            WorkspaceId = board.WorkspaceId,
            Name = board.Name,
            CreatedByUsername = board.CreatedBy?.Username ?? "Unknown",
            CreatedAt = board.CreatedAt,
            IsArchived = board.IsArchived
        };
    }
}
