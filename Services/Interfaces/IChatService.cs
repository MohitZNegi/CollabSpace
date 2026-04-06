using CollabSpace.Models.DTOs.Chat;

namespace CollabSpace.Services.Interfaces
{
    public interface IChatService
    {
        // Workspace chat
        Task<MessageResponseDto> SendWorkspaceMessageAsync(
            Guid workspaceId, SendMessageDto request, Guid senderId);

        Task<List<MessageResponseDto>> GetWorkspaceMessagesAsync(
            Guid workspaceId, Guid requestingUserId,
            DateTime? before = null, int limit = 50);

        Task<MessageResponseDto> EditWorkspaceMessageAsync(
            Guid messageId, string newContent, Guid requestingUserId);

        // Direct messages
        Task<DirectMessageResponseDto> SendDirectMessageAsync(
            Guid recipientId, SendMessageDto request, Guid senderId);

        Task<List<DirectMessageResponseDto>> GetDirectMessagesAsync(
            Guid otherUserId, Guid requestingUserId,
            DateTime? before = null, int limit = 50);

        Task MarkDirectMessagesAsReadAsync(
            Guid senderId, Guid recipientId);
    }
}