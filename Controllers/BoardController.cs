using CollabSpace.Models.DTOs.Board;
using CollabSpace.Models.DTOs.Card;
using CollabSpace.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CollabSpace.Controllers
{
    [ApiController]
    [Authorize]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;
        private readonly ICardService _cardService;
        private readonly ICurrentUserService _currentUser;

        public BoardController(
            IBoardService boardService,
            ICardService cardService,
            ICurrentUserService currentUser)
        {
            _boardService = boardService;
            _cardService = cardService;
            _currentUser = currentUser;
        }

        // GET /api/v1/workspaces/{workspaceId}/boards
        [HttpGet("api/v1/workspaces/{workspaceId:guid}/boards")]
        public async Task<IActionResult> GetBoards(Guid workspaceId)
        {
            var result = await _boardService.GetBoardsAsync(
                workspaceId, _currentUser.GetUserId());
            return Ok(result);
        }

        // POST /api/v1/workspaces/{workspaceId}/boards
        [HttpPost("api/v1/workspaces/{workspaceId:guid}/boards")]
        public async Task<IActionResult> CreateBoard(
            Guid workspaceId, [FromBody] CreateBoardDto request)
        {
            var result = await _boardService.CreateBoardAsync(
                workspaceId, request, _currentUser.GetUserId());
            return StatusCode(201, result);
        }

        // DELETE /api/v1/boards/{boardId}
        [HttpDelete("api/v1/boards/{boardId:guid}")]
        public async Task<IActionResult> ArchiveBoard(Guid boardId)
        {
            await _boardService.ArchiveBoardAsync(
                boardId, _currentUser.GetUserId());
            return NoContent();
        }

        // GET /api/v1/boards/{boardId}/cards
        [HttpGet("api/v1/boards/{boardId:guid}/cards")]
        public async Task<IActionResult> GetCards(Guid boardId)
        {
            var result = await _cardService.GetCardsAsync(
                boardId, _currentUser.GetUserId());
            return Ok(result);
        }

        // POST /api/v1/boards/{boardId}/cards
        [HttpPost("api/v1/boards/{boardId:guid}/cards")]
        public async Task<IActionResult> CreateCard(
            Guid boardId, [FromBody] CreateCardDto request)
        {
            var result = await _cardService.CreateCardAsync(
                boardId, request, _currentUser.GetUserId());
            return StatusCode(201, result);
        }

        // PUT /api/v1/cards/{cardId}
        [HttpPut("api/v1/cards/{cardId:guid}")]
        public async Task<IActionResult> UpdateCard(
            Guid cardId, [FromBody] UpdateCardDto request)
        {
            var result = await _cardService.UpdateCardAsync(
                cardId, request, _currentUser.GetUserId());
            return Ok(result);
        }

        // PATCH /api/v1/cards/{cardId}/move
        [HttpPatch("api/v1/cards/{cardId:guid}/move")]
        public async Task<IActionResult> MoveCard(
            Guid cardId, [FromBody] MoveCardDto request)
        {
            await _cardService.MoveCardAsync(
                cardId, request, _currentUser.GetUserId());
            return Ok();
        }

        // DELETE /api/v1/cards/{cardId}
        [HttpDelete("api/v1/cards/{cardId:guid}")]
        public async Task<IActionResult> DeleteCard(Guid cardId)
        {
            await _cardService.DeleteCardAsync(
                cardId, _currentUser.GetUserId());
            return NoContent();
        }
    }
}