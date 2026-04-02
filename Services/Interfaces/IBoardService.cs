using CollabSpace.Models.DTOs.Board;

namespace CollabSpace.Services.Interfaces
{
    public interface IBoardService
    {
        Task<List<BoardResponseDto>> GetBoardsAsync(
            Guid workspaceId, Guid requestingUserId);

        Task<BoardResponseDto> CreateBoardAsync(
            Guid workspaceId, CreateBoardDto request, Guid createdByUserId);

        Task ArchiveBoardAsync(Guid boardId, Guid requestingUserId);
    }
}