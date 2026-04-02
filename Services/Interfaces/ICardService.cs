using CollabSpace.Models.DTOs.Card;

namespace CollabSpace.Services.Interfaces
{
    public interface ICardService
    {
        Task<List<CardResponseDto>> GetCardsAsync(
            Guid boardId, Guid requestingUserId);

        Task<CardResponseDto> CreateCardAsync(
            Guid boardId, CreateCardDto request, Guid createdByUserId);

        Task<CardResponseDto> UpdateCardAsync(
            Guid cardId, UpdateCardDto request, Guid requestingUserId);

        Task MoveCardAsync(
            Guid cardId, MoveCardDto request, Guid requestingUserId);

        Task DeleteCardAsync(Guid cardId, Guid requestingUserId);
    }
}