using CollabSpace.Data;
using CollabSpace.Exceptions;
using CollabSpace.Models;
using CollabSpace.Models.DTOs.Chat;
using CollabSpace.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CollabSpace.Services
{
    public class ChatService : IChatService
    {
        private readonly AppDbContext _context;
        private readonly IChatEventService _chatEvents;

        public ChatService(AppDbContext context, IChatEventService chatEvents)
        {
            _context = context;
            _chatEvents = chatEvents;
        }

        // ---------------------------------------------------------------
        // WORKSPACE CHAT
        // ---------------------------------------------------------------

        public async Task<MessageResponseDto> SendWorkspaceMessageAsync(
            Guid workspaceId, SendMessageDto request, Guid senderId)
        {
            // Verify membership before allowing any message
            await RequireWorkspaceMemberAsync(workspaceId, senderId);

            var message = new Message
            {
                Id = Guid.NewGuid(),
                WorkspaceId = workspaceId,
                SenderId = senderId,
                Content = request.Content.Trim(),
                SentAt = DateTime.UtcNow,
                IsEdited = false
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Load the sender for the response DTO
            await _context.Entry(message)
                .Reference(m => m.Sender).LoadAsync();

            var dto = MapToMessageDto(message);

            // Persist first, broadcast second. This order is critical.
            // If SignalR fails, the message is still in the database.
            // If the database fails, no message is broadcast.
            await _chatEvents.BroadcastWorkspaceMessageAsync(
                workspaceId.ToString(), dto);

            return dto;
        }

        public async Task<List<MessageResponseDto>> GetWorkspaceMessagesAsync(
            Guid workspaceId, Guid requestingUserId,
            DateTime? before = null, int limit = 50)
        {
            await RequireWorkspaceMemberAsync(workspaceId, requestingUserId);

            var query = _context.Messages
                .Where(m => m.WorkspaceId == workspaceId)
                .Include(m => m.Sender)
                .AsQueryable();

            // Cursor-based pagination using timestamps.
            // "Give me 50 messages before this point in time."
            // This is more efficient than offset pagination for chat
            // because it does not scan from the beginning each time.
            if (before.HasValue)
                query = query.Where(m => m.SentAt < before.Value);

            return await query
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                // Reverse so oldest is first in the returned list.
                // The UI renders from top (oldest) to bottom (newest).
                .OrderBy(m => m.SentAt)
                .Select(m => MapToMessageDto(m))
                .ToListAsync();
        }

        public async Task<MessageResponseDto> EditWorkspaceMessageAsync(
            Guid messageId, string newContent, Guid requestingUserId)
        {
            var message = await _context.Messages
                .Include(m => m.Sender)
                .FirstOrDefaultAsync(m => m.Id == messageId);

            if (message == null)
                throw new KeyNotFoundException("Message not found.");

            // Only the sender can edit their own message
            if (message.SenderId != requestingUserId)
                throw new ForbiddenException(
                    "You can only edit your own messages.");

            message.Content = newContent.Trim();
            message.IsEdited = true;

            await _context.SaveChangesAsync();

            var dto = MapToMessageDto(message);

            // Broadcast the edit so all users see the updated content
            await _chatEvents.BroadcastWorkspaceMessageAsync(
                message.WorkspaceId.ToString(), dto);

            return dto;
        }

        // ---------------------------------------------------------------
        // DIRECT MESSAGES
        // ---------------------------------------------------------------

        public async Task<DirectMessageResponseDto> SendDirectMessageAsync(
            Guid recipientId, SendMessageDto request, Guid senderId)
        {
            // Prevent messaging yourself
            if (senderId == recipientId)
                throw new ArgumentException(
                    "You cannot send a direct message to yourself.");

            // Verify both users exist and are active
            var recipientExists = await _context.Users
                .AnyAsync(u => u.Id == recipientId && u.IsActive);

            if (!recipientExists)
                throw new KeyNotFoundException("Recipient not found.");

            var dm = new DirectMessage
            {
                Id = Guid.NewGuid(),
                SenderId = senderId,
                RecipientId = recipientId,
                Content = request.Content.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _context.DirectMessages.Add(dm);
            await _context.SaveChangesAsync();

            await _context.Entry(dm).Reference(d => d.Sender).LoadAsync();

            var dto = MapToDmDto(dm);

            // Target delivery: only the recipient receives this event.
            // The sender sees it immediately because they sent it.
            await _chatEvents.SendDirectMessageAsync(
                recipientId.ToString(), dto);

            return dto;
        }

        public async Task<List<DirectMessageResponseDto>> GetDirectMessagesAsync(
            Guid otherUserId, Guid requestingUserId,
            DateTime? before = null, int limit = 50)
        {
            var query = _context.DirectMessages
                // Load messages in both directions between these two users
                .Where(dm =>
                    (dm.SenderId == requestingUserId
                        && dm.RecipientId == otherUserId)
                    || (dm.SenderId == otherUserId
                        && dm.RecipientId == requestingUserId))
                .Include(dm => dm.Sender)
                .AsQueryable();

            if (before.HasValue)
                query = query.Where(dm => dm.SentAt < before.Value);

            return await query
                .OrderByDescending(dm => dm.SentAt)
                .Take(limit)
                .OrderBy(dm => dm.SentAt)
                .Select(dm => MapToDmDto(dm))
                .ToListAsync();
        }

        public async Task MarkDirectMessagesAsReadAsync(
            Guid senderId, Guid recipientId)
        {
            // Mark all unread messages FROM senderId TO recipientId as read.
            // Called when the recipient opens the conversation.
            var unreadMessages = await _context.DirectMessages
                .Where(dm => dm.SenderId == senderId
                          && dm.RecipientId == recipientId
                          && !dm.IsRead)
                .ToListAsync();

            foreach (var dm in unreadMessages)
                dm.IsRead = true;

            if (unreadMessages.Any())
                await _context.SaveChangesAsync();
        }

        // ---------------------------------------------------------------
        // PRIVATE HELPERS
        // ---------------------------------------------------------------

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

        private static MessageResponseDto MapToMessageDto(Message m) => new()
        {
            Id = m.Id,
            WorkspaceId = m.WorkspaceId,
            SenderId = m.SenderId,
            SenderUsername = m.Sender?.Username ?? "Unknown",
            SenderAvatarUrl = m.Sender?.AvatarUrl,
            Content = m.Content,
            SentAt = m.SentAt,
            IsEdited = m.IsEdited
        };

        private static DirectMessageResponseDto MapToDmDto(DirectMessage dm) => new()
        {
            Id = dm.Id,
            SenderId = dm.SenderId,
            SenderUsername = dm.Sender?.Username ?? "Unknown",
            SenderAvatarUrl = dm.Sender?.AvatarUrl,
            RecipientId = dm.RecipientId,
            Content = dm.Content,
            SentAt = dm.SentAt,
            IsRead = dm.IsRead
        };
    }
}